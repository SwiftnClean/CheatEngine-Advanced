using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheatEngine.NET.Core;
using CheatEngine.NET.Memory;
using CheatEngine.NET.Utils;

namespace CheatEngine.NET.Scanner
{
    /// <summary>
    /// Handles memory scanning operations
    /// </summary>
    public class MemoryScanner
    {
        private List<ScanResult> _currentResults = new List<ScanResult>();
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isScanning = false;
        private ScanSettings _lastScanSettings = new ScanSettings();
        
        /// <summary>
        /// Event raised when a scan is completed
        /// </summary>
        public event EventHandler<ScanCompleteEventArgs>? ScanComplete;
        
        /// <summary>
        /// Event raised to report scan progress
        /// </summary>
        public event EventHandler<ScanProgressEventArgs>? ScanProgress;
        
        /// <summary>
        /// Gets a value indicating whether a scan is currently in progress
        /// </summary>
        public bool IsScanning => _isScanning;
        
        /// <summary>
        /// Gets the current scan results
        /// </summary>
        public IReadOnlyList<ScanResult> CurrentResults => _currentResults.AsReadOnly();
        
        /// <summary>
        /// Starts a new memory scan
        /// </summary>
        /// <param name="scanType">The type of scan to perform</param>
        /// <param name="scanValue">The value to scan for</param>
        public void StartScan(ScanType scanType, string scanValue)
        {
            if (_isScanning)
            {
                throw new InvalidOperationException("A scan is already in progress");
            }
            
            if (CheatEngineCore.MemoryManager == null || CheatEngineCore.TargetProcess == null)
            {
                throw new InvalidOperationException("No process is attached");
            }
            
            _isScanning = true;
            _currentResults.Clear();
            
            // Create scan settings
            _lastScanSettings = new ScanSettings
            {
                ScanType = scanType,
                ScanValue = scanValue,
                IsFirstScan = true
            };
            
            // Start scan in background
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => PerformScan(_lastScanSettings, _cancellationTokenSource.Token));
        }
        
        /// <summary>
        /// Starts a next scan using the previous results
        /// </summary>
        /// <param name="scanType">The type of scan to perform</param>
        /// <param name="scanValue">The value to scan for</param>
        public void StartNextScan(ScanType scanType, string scanValue)
        {
            if (_isScanning)
            {
                throw new InvalidOperationException("A scan is already in progress");
            }
            
            if (_currentResults.Count == 0)
            {
                throw new InvalidOperationException("No previous scan results to filter");
            }
            
            if (CheatEngineCore.MemoryManager == null || CheatEngineCore.TargetProcess == null)
            {
                throw new InvalidOperationException("No process is attached");
            }
            
            _isScanning = true;
            
            // Create scan settings
            _lastScanSettings = new ScanSettings
            {
                ScanType = scanType,
                ScanValue = scanValue,
                IsFirstScan = false
            };
            
            // Start scan in background
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => PerformScan(_lastScanSettings, _cancellationTokenSource.Token));
        }
        
        /// <summary>
        /// Stops the current scan
        /// </summary>
        public void StopScan()
        {
            if (!_isScanning || _cancellationTokenSource == null)
            {
                return;
            }
            
            _cancellationTokenSource.Cancel();
        }
        
        /// <summary>
        /// Performs the actual memory scan
        /// </summary>
        /// <param name="settings">The scan settings</param>
        /// <param name="cancellationToken">Cancellation token</param>
        private void PerformScan(ScanSettings settings, CancellationToken cancellationToken)
        {
            try
            {
                List<ScanResult> results = new List<ScanResult>();
                
                if (settings.IsFirstScan)
                {
                    // First scan - scan all memory regions
                    results = PerformFirstScan(settings, cancellationToken);
                }
                else
                {
                    // Next scan - filter existing results
                    results = PerformNextScan(settings, cancellationToken);
                }
                
                // Update current results
                _currentResults = results;
                
                // Raise scan complete event
                _isScanning = false;
                OnScanComplete(new ScanCompleteEventArgs(results.Count, results));
            }
            catch (OperationCanceledException)
            {
                // Scan was cancelled
                _isScanning = false;
                OnScanComplete(new ScanCompleteEventArgs(_currentResults.Count, _currentResults));
            }
            catch (Exception ex)
            {
                // Scan failed
                Logger.Log($"Scan failed: {ex.Message}", LogLevel.Error);
                _isScanning = false;
                OnScanComplete(new ScanCompleteEventArgs(0, new List<ScanResult>()));
            }
        }
        
        /// <summary>
        /// Performs a first scan of all memory regions
        /// </summary>
        /// <param name="settings">The scan settings</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The scan results</returns>
        private List<ScanResult> PerformFirstScan(ScanSettings settings, CancellationToken cancellationToken)
        {
            List<ScanResult> results = new List<ScanResult>();
            
            // Get all memory regions
            var memoryManager = CheatEngineCore.MemoryManager!;
            var memoryRegions = memoryManager.GetMemoryRegions();
            
            // Filter regions based on scan settings
            var regionsToScan = memoryRegions.Where(r => r.IsReadable).ToList();
            
            // Calculate total memory to scan
            long totalMemory = regionsToScan.Sum(r => (long)r.Size);
            long scannedMemory = 0;
            
            // Scan each region
            foreach (var region in regionsToScan)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }
                
                try
                {
                    // Scan region
                    var regionResults = ScanRegion(region, settings);
                    results.AddRange(regionResults);
                    
                    // Update progress
                    scannedMemory += region.Size;
                    int progressPercentage = (int)((scannedMemory * 100) / totalMemory);
                    OnScanProgress(new ScanProgressEventArgs(progressPercentage));
                }
                catch (Exception ex)
                {
                    // Log error and continue with next region
                    Logger.Log($"Error scanning region at 0x{region.BaseAddress.ToInt64():X}: {ex.Message}", LogLevel.Warning);
                }
            }
            
            return results;
        }
        
        /// <summary>
        /// Performs a next scan using the previous results
        /// </summary>
        /// <param name="settings">The scan settings</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The filtered scan results</returns>
        private List<ScanResult> PerformNextScan(ScanSettings settings, CancellationToken cancellationToken)
        {
            List<ScanResult> results = new List<ScanResult>();
            
            // Get memory manager
            var memoryManager = CheatEngineCore.MemoryManager!;
            
            // Calculate total results to scan
            int totalResults = _currentResults.Count;
            int scannedResults = 0;
            
            // Scan each previous result
            foreach (var result in _currentResults)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }
                
                try
                {
                    // Check if the value at this address matches the new scan criteria
                    bool matches = CheckValueMatch(result.Address, result.ValueType, settings);
                    
                    if (matches)
                    {
                        // Update the result with the current value
                        var updatedResult = new ScanResult
                        {
                            Address = result.Address,
                            ValueType = result.ValueType,
                            Value = ReadValueAsString(result.Address, result.ValueType),
                            Description = result.Description
                        };
                        
                        results.Add(updatedResult);
                    }
                    
                    // Update progress
                    scannedResults++;
                    int progressPercentage = (int)((scannedResults * 100) / totalResults);
                    OnScanProgress(new ScanProgressEventArgs(progressPercentage));
                }
                catch (Exception ex)
                {
                    // Log error and continue with next result
                    Logger.Log($"Error checking value at 0x{result.Address.ToInt64():X}: {ex.Message}", LogLevel.Warning);
                }
            }
            
            return results;
        }
        
        /// <summary>
        /// Scans a memory region for values matching the scan criteria
        /// </summary>
        /// <param name="region">The memory region to scan</param>
        /// <param name="settings">The scan settings</param>
        /// <returns>The scan results for the region</returns>
        private List<ScanResult> ScanRegion(MemoryRegion region, ScanSettings settings)
        {
            List<ScanResult> results = new List<ScanResult>();
            
            // Get memory manager
            var memoryManager = CheatEngineCore.MemoryManager!;
            
            // Determine value size based on scan type
            int valueSize = GetValueSize(settings.ScanType);
            
            // Read the entire region
            byte[] buffer;
            try
            {
                buffer = memoryManager.ReadMemory(region.BaseAddress, region.Size);
            }
            catch
            {
                // Skip regions that can't be read
                return results;
            }
            
            // Scan the buffer for matching values
            for (int offset = 0; offset <= buffer.Length - valueSize; offset++)
            {
                IntPtr address = new IntPtr(region.BaseAddress.ToInt64() + offset);
                
                try
                {
                    // Check if the value at this offset matches the scan criteria
                    bool matches = CheckBufferMatch(buffer, offset, settings);
                    
                    if (matches)
                    {
                        // Add result
                        var result = new ScanResult
                        {
                            Address = address,
                            ValueType = settings.ScanType,
                            Value = ReadValueAsString(address, settings.ScanType),
                            Description = string.Empty
                        };
                        
                        results.Add(result);
                    }
                }
                catch
                {
                    // Skip addresses that can't be read
                }
            }
            
            return results;
        }
        
        /// <summary>
        /// Checks if a value in a buffer matches the scan criteria
        /// </summary>
        /// <param name="buffer">The memory buffer</param>
        /// <param name="offset">The offset within the buffer</param>
        /// <param name="settings">The scan settings</param>
        /// <returns>True if the value matches, false otherwise</returns>
        private bool CheckBufferMatch(byte[] buffer, int offset, ScanSettings settings)
        {
            // Extract value from buffer based on scan type
            switch (settings.ScanType)
            {
                case ScanType.Byte:
                    byte byteValue = buffer[offset];
                    return CompareValue(byteValue, settings.ScanValue, settings.ScanType);
                    
                case ScanType.Short:
                    if (offset + 1 >= buffer.Length) return false;
                    short shortValue = BitConverter.ToInt16(buffer, offset);
                    return CompareValue(shortValue, settings.ScanValue, settings.ScanType);
                    
                case ScanType.Integer:
                    if (offset + 3 >= buffer.Length) return false;
                    int intValue = BitConverter.ToInt32(buffer, offset);
                    return CompareValue(intValue, settings.ScanValue, settings.ScanType);
                    
                case ScanType.Long:
                    if (offset + 7 >= buffer.Length) return false;
                    long longValue = BitConverter.ToInt64(buffer, offset);
                    return CompareValue(longValue, settings.ScanValue, settings.ScanType);
                    
                case ScanType.Float:
                    if (offset + 3 >= buffer.Length) return false;
                    float floatValue = BitConverter.ToSingle(buffer, offset);
                    return CompareValue(floatValue, settings.ScanValue, settings.ScanType);
                    
                case ScanType.Double:
                    if (offset + 7 >= buffer.Length) return false;
                    double doubleValue = BitConverter.ToDouble(buffer, offset);
                    return CompareValue(doubleValue, settings.ScanValue, settings.ScanType);
                    
                case ScanType.String:
                    // String scanning is more complex and would require additional logic
                    return false;
                    
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Checks if a value at an address matches the scan criteria
        /// </summary>
        /// <param name="address">The memory address</param>
        /// <param name="valueType">The type of value</param>
        /// <param name="settings">The scan settings</param>
        /// <returns>True if the value matches, false otherwise</returns>
        private bool CheckValueMatch(IntPtr address, ScanType valueType, ScanSettings settings)
        {
            // Get memory manager
            var memoryManager = CheatEngineCore.MemoryManager!;
            
            try
            {
                // Read value from memory based on value type
                switch (valueType)
                {
                    case ScanType.Byte:
                        byte byteValue = memoryManager.ReadValue<byte>(address);
                        return CompareValue(byteValue, settings.ScanValue, settings.ScanType);
                        
                    case ScanType.Short:
                        short shortValue = memoryManager.ReadValue<short>(address);
                        return CompareValue(shortValue, settings.ScanValue, settings.ScanType);
                        
                    case ScanType.Integer:
                        int intValue = memoryManager.ReadValue<int>(address);
                        return CompareValue(intValue, settings.ScanValue, settings.ScanType);
                        
                    case ScanType.Long:
                        long longValue = memoryManager.ReadValue<long>(address);
                        return CompareValue(longValue, settings.ScanValue, settings.ScanType);
                        
                    case ScanType.Float:
                        float floatValue = memoryManager.ReadValue<float>(address);
                        return CompareValue(floatValue, settings.ScanValue, settings.ScanType);
                        
                    case ScanType.Double:
                        double doubleValue = memoryManager.ReadValue<double>(address);
                        return CompareValue(doubleValue, settings.ScanValue, settings.ScanType);
                        
                    case ScanType.String:
                        // String scanning is more complex and would require additional logic
                        return false;
                        
                    default:
                        return false;
                }
            }
            catch
            {
                // Skip addresses that can't be read
                return false;
            }
        }
        
        /// <summary>
        /// Compares a value with a scan value
        /// </summary>
        /// <param name="value">The value to compare</param>
        /// <param name="scanValue">The scan value as a string</param>
        /// <param name="scanType">The type of scan</param>
        /// <returns>True if the value matches, false otherwise</returns>
        private bool CompareValue<T>(T value, string scanValue, ScanType scanType)
        {
            try
            {
                // Parse scan value based on scan type
                switch (scanType)
                {
                    case ScanType.Byte:
                        byte byteValue = byte.Parse(scanValue);
                        return value!.Equals(byteValue);
                        
                    case ScanType.Short:
                        short shortValue = short.Parse(scanValue);
                        return value!.Equals(shortValue);
                        
                    case ScanType.Integer:
                        int intValue = int.Parse(scanValue);
                        return value!.Equals(intValue);
                        
                    case ScanType.Long:
                        long longValue = long.Parse(scanValue);
                        return value!.Equals(longValue);
                        
                    case ScanType.Float:
                        float floatValue = float.Parse(scanValue);
                        return value!.Equals(floatValue);
                        
                    case ScanType.Double:
                        double doubleValue = double.Parse(scanValue);
                        return value!.Equals(doubleValue);
                        
                    case ScanType.String:
                        // String comparison is more complex and would require additional logic
                        return value!.Equals(scanValue);
                        
                    default:
                        return false;
                }
            }
            catch
            {
                // Invalid scan value
                return false;
            }
        }
        
        /// <summary>
        /// Gets the size of a value type in bytes
        /// </summary>
        /// <param name="valueType">The value type</param>
        /// <returns>The size in bytes</returns>
        private int GetValueSize(ScanType valueType)
        {
            switch (valueType)
            {
                case ScanType.Byte:
                    return 1;
                case ScanType.Short:
                    return 2;
                case ScanType.Integer:
                case ScanType.Float:
                    return 4;
                case ScanType.Long:
                case ScanType.Double:
                    return 8;
                case ScanType.String:
                    // Variable size, use a default
                    return 1;
                default:
                    return 1;
            }
        }
        
        /// <summary>
        /// Reads a value from memory as a string
        /// </summary>
        /// <param name="address">The memory address</param>
        /// <param name="valueType">The type of value</param>
        /// <returns>The value as a string</returns>
        private string ReadValueAsString(IntPtr address, ScanType valueType)
        {
            // Get memory manager
            var memoryManager = CheatEngineCore.MemoryManager!;
            
            try
            {
                // Read value from memory based on value type
                switch (valueType)
                {
                    case ScanType.Byte:
                        byte byteValue = memoryManager.ReadValue<byte>(address);
                        return byteValue.ToString();
                        
                    case ScanType.Short:
                        short shortValue = memoryManager.ReadValue<short>(address);
                        return shortValue.ToString();
                        
                    case ScanType.Integer:
                        int intValue = memoryManager.ReadValue<int>(address);
                        return intValue.ToString();
                        
                    case ScanType.Long:
                        long longValue = memoryManager.ReadValue<long>(address);
                        return longValue.ToString();
                        
                    case ScanType.Float:
                        float floatValue = memoryManager.ReadValue<float>(address);
                        return floatValue.ToString("F6");
                        
                    case ScanType.Double:
                        double doubleValue = memoryManager.ReadValue<double>(address);
                        return doubleValue.ToString("F6");
                        
                    case ScanType.String:
                        // Read a string (limited to 100 characters)
                        return memoryManager.ReadString(address, 100, Encoding.ASCII);
                        
                    default:
                        return "Unknown";
                }
            }
            catch
            {
                return "Error";
            }
        }
        
        /// <summary>
        /// Raises the ScanComplete event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnScanComplete(ScanCompleteEventArgs e)
        {
            ScanComplete?.Invoke(this, e);
        }
        
        /// <summary>
        /// Raises the ScanProgress event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnScanProgress(ScanProgressEventArgs e)
        {
            ScanProgress?.Invoke(this, e);
        }
    }
    
    /// <summary>
    /// Types of memory scans
    /// </summary>
    public enum ScanType
    {
        Byte,
        Short,
        Integer,
        Long,
        Float,
        Double,
        String
    }
    
    /// <summary>
    /// Settings for a memory scan
    /// </summary>
    public class ScanSettings
    {
        /// <summary>
        /// Gets or sets the type of scan
        /// </summary>
        public ScanType ScanType { get; set; }
        
        /// <summary>
        /// Gets or sets the value to scan for
        /// </summary>
        public string ScanValue { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets a value indicating whether this is the first scan
        /// </summary>
        public bool IsFirstScan { get; set; }
    }
    
    /// <summary>
    /// Result of a memory scan
    /// </summary>
    public class ScanResult
    {
        /// <summary>
        /// Gets or sets the memory address
        /// </summary>
        public IntPtr Address { get; set; }
        
        /// <summary>
        /// Gets or sets the type of value
        /// </summary>
        public ScanType ValueType { get; set; }
        
        /// <summary>
        /// Gets or sets the value as a string
        /// </summary>
        public string Value { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Event arguments for scan complete event
    /// </summary>
    public class ScanCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the number of results
        /// </summary>
        public int ResultCount { get; }
        
        /// <summary>
        /// Gets the scan results
        /// </summary>
        public IReadOnlyList<ScanResult> Results { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ScanCompleteEventArgs"/> class
        /// </summary>
        /// <param name="resultCount">The number of results</param>
        /// <param name="results">The scan results</param>
        public ScanCompleteEventArgs(int resultCount, IReadOnlyList<ScanResult> results)
        {
            ResultCount = resultCount;
            Results = results;
        }
    }
    
    /// <summary>
    /// Event arguments for scan progress event
    /// </summary>
    public class ScanProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the progress percentage
        /// </summary>
        public int ProgressPercentage { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ScanProgressEventArgs"/> class
        /// </summary>
        /// <param name="progressPercentage">The progress percentage</param>
        public ScanProgressEventArgs(int progressPercentage)
        {
            ProgressPercentage = progressPercentage;
        }
    }
}
