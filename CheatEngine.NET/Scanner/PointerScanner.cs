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
    /// Handles pointer scanning operations
    /// </summary>
    public class PointerScanner
    {
        private List<PointerPath> _currentResults = new List<PointerPath>();
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isScanning = false;
        private PointerScanSettings _lastScanSettings = new PointerScanSettings();
        
        /// <summary>
        /// Event raised when a scan is completed
        /// </summary>
        public event EventHandler<PointerScanCompleteEventArgs>? ScanComplete;
        
        /// <summary>
        /// Event raised when a scan is completed (alias for ScanComplete for backward compatibility)
        /// </summary>
        public event EventHandler<PointerScanCompleteEventArgs>? PointerScanComplete
        {
            add { ScanComplete += value; }
            remove { ScanComplete -= value; }
        }
        
        /// <summary>
        /// Event raised to report scan progress
        /// </summary>
        public event EventHandler<PointerScanProgressEventArgs>? ScanProgress;
        
        /// <summary>
        /// Event raised to report scan progress (alias for ScanProgress for backward compatibility)
        /// </summary>
        public event EventHandler<PointerScanProgressEventArgs>? PointerScanProgress
        {
            add { ScanProgress += value; }
            remove { ScanProgress -= value; }
        }
        
        /// <summary>
        /// Gets a value indicating whether a scan is currently in progress
        /// </summary>
        public bool IsScanning => _isScanning;
        
        /// <summary>
        /// Gets the current scan results
        /// </summary>
        public IReadOnlyList<PointerPath> CurrentResults => _currentResults.AsReadOnly();
        
        /// <summary>
        /// Starts a new pointer scan asynchronously
        /// </summary>
        /// <param name="targetAddress">The target address to find pointers to</param>
        /// <param name="maxLevel">The maximum pointer level</param>
        /// <param name="maxOffset">The maximum offset value</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task ScanForPointersAsync(IntPtr targetAddress, int maxLevel = 3, int maxOffset = 4096)
        {
            await Task.Run(() => StartScan(targetAddress, maxLevel, maxOffset));
        }
        
        /// <summary>
        /// Starts a new pointer scan
        /// </summary>
        /// <param name="targetAddress">The target address to find pointers to</param>
        /// <param name="maxLevel">The maximum pointer level</param>
        /// <param name="maxOffset">The maximum offset value</param>
        public void StartScan(IntPtr targetAddress, int maxLevel = 3, int maxOffset = 4096)
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
            _lastScanSettings = new PointerScanSettings
            {
                TargetAddress = targetAddress,
                MaxLevel = maxLevel,
                MaxOffset = maxOffset
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
        /// Performs the actual pointer scan
        /// </summary>
        /// <param name="settings">The scan settings</param>
        /// <param name="cancellationToken">Cancellation token</param>
        private void PerformScan(PointerScanSettings settings, CancellationToken cancellationToken)
        {
            try
            {
                List<PointerPath> results = new List<PointerPath>();
                
                // Get all memory regions
                var memoryManager = CheatEngineCore.MemoryManager!;
                var memoryRegions = memoryManager.GetMemoryRegions();
                
                // Filter regions that could contain pointers (readable)
                var regionsToScan = memoryRegions.Where(r => r.IsReadable).ToList();
                
                // Calculate total memory to scan
                long totalMemory = regionsToScan.Sum(r => (long)r.Size);
                long scannedMemory = 0;
                
                // Find direct pointers first (level 1)
                List<PointerInfo> directPointers = FindDirectPointers(settings.TargetAddress, regionsToScan, settings.MaxOffset, cancellationToken, ref scannedMemory, totalMemory);
                
                // Create pointer paths from direct pointers
                foreach (var pointer in directPointers)
                {
                    PointerPath path = new PointerPath
                    {
                        BaseAddress = pointer.Address,
                        Offsets = new List<int> { pointer.Offset },
                        TargetAddress = settings.TargetAddress
                    };
                    
                    results.Add(path);
                }
                
                // Find multi-level pointers if requested
                if (settings.MaxLevel > 1)
                {
                    for (int level = 2; level <= settings.MaxLevel; level++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException();
                        }
                        
                        // Find pointers to the base addresses of the current results
                        List<PointerPath> newPaths = new List<PointerPath>();
                        
                        foreach (var path in results.Where(p => p.Offsets.Count == level - 1).ToList())
                        {
                            // Find pointers to this base address
                            List<PointerInfo> nextLevelPointers = FindDirectPointers(path.BaseAddress, regionsToScan, settings.MaxOffset, cancellationToken, ref scannedMemory, totalMemory);
                            
                            foreach (var pointer in nextLevelPointers)
                            {
                                // Create new path with this pointer as the base
                                PointerPath newPath = new PointerPath
                                {
                                    BaseAddress = pointer.Address,
                                    Offsets = new List<int> { pointer.Offset },
                                    TargetAddress = settings.TargetAddress
                                };
                                
                                // Add existing offsets
                                newPath.Offsets.AddRange(path.Offsets);
                                
                                newPaths.Add(newPath);
                            }
                        }
                        
                        // Add new paths to results
                        results.AddRange(newPaths);
                        
                        // Report progress
                        int progressPercentage = (int)((level * 100) / settings.MaxLevel);
                        OnScanProgress(new PointerScanProgressEventArgs(progressPercentage));
                    }
                }
                
                // Update current results
                _currentResults = results;
                
                // Raise scan complete event
                _isScanning = false;
                OnScanComplete(new PointerScanCompleteEventArgs(results.Count, results));
            }
            catch (OperationCanceledException)
            {
                // Scan was cancelled
                _isScanning = false;
                OnScanComplete(new PointerScanCompleteEventArgs(_currentResults.Count, _currentResults));
            }
            catch (Exception ex)
            {
                // Scan failed
                Logger.Log($"Pointer scan failed: {ex.Message}", LogLevel.Error);
                _isScanning = false;
                OnScanComplete(new PointerScanCompleteEventArgs(0, new List<PointerPath>()));
            }
        }
        
        /// <summary>
        /// Finds direct pointers to an address
        /// </summary>
        /// <param name="targetAddress">The target address</param>
        /// <param name="regions">The memory regions to scan</param>
        /// <param name="maxOffset">The maximum offset</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="scannedMemory">The amount of memory scanned so far</param>
        /// <param name="totalMemory">The total amount of memory to scan</param>
        /// <returns>List of pointers to the target address</returns>
        private List<PointerInfo> FindDirectPointers(IntPtr targetAddress, List<MemoryRegion> regions, int maxOffset, CancellationToken cancellationToken, ref long scannedMemory, long totalMemory)
        {
            List<PointerInfo> pointers = new List<PointerInfo>();
            
            // Get memory manager
            var memoryManager = CheatEngineCore.MemoryManager!;
            
            // Calculate address range to look for
            long minAddressValue = targetAddress.ToInt64() - maxOffset;
            long maxAddressValue = targetAddress.ToInt64() + maxOffset;
            
            // Scan each region
            foreach (var region in regions)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }
                
                try
                {
                    // Read the entire region
                    byte[] buffer;
                    try
                    {
                        buffer = memoryManager.ReadMemory(region.BaseAddress, region.Size);
                    }
                    catch
                    {
                        // Skip regions that can't be read
                        continue;
                    }
                    
                    // Scan for pointers
                    int pointerSize = IntPtr.Size;
                    for (int offset = 0; offset <= buffer.Length - pointerSize; offset += pointerSize)
                    {
                        // Read potential pointer value
                        long pointerValue;
                        if (pointerSize == 4)
                        {
                            pointerValue = BitConverter.ToInt32(buffer, offset);
                        }
                        else
                        {
                            pointerValue = BitConverter.ToInt64(buffer, offset);
                        }
                        
                        // Check if it points to our target address range
                        if (pointerValue >= minAddressValue && pointerValue <= maxAddressValue)
                        {
                            // Calculate offset from the pointer value to the target address
                            int pointerOffset = (int)(targetAddress.ToInt64() - pointerValue);
                            
                            // Add to results
                            IntPtr pointerAddress = new IntPtr(region.BaseAddress.ToInt64() + offset);
                            pointers.Add(new PointerInfo
                            {
                                Address = pointerAddress,
                                PointsTo = new IntPtr(pointerValue),
                                Offset = pointerOffset
                            });
                        }
                    }
                    
                    // Update progress
                    scannedMemory += region.Size;
                    int progressPercentage = (int)((scannedMemory * 100) / totalMemory);
                    OnScanProgress(new PointerScanProgressEventArgs(progressPercentage));
                }
                catch (Exception ex)
                {
                    // Log error and continue with next region
                    Logger.Log($"Error scanning region at 0x{region.BaseAddress.ToInt64():X}: {ex.Message}", LogLevel.Warning);
                }
            }
            
            return pointers;
        }
        
        /// <summary>
        /// Resolves a pointer path to get the final address
        /// </summary>
        /// <param name="path">The pointer path</param>
        /// <returns>The resolved address</returns>
        public IntPtr ResolvePointerPath(PointerPath path)
        {
            if (CheatEngineCore.MemoryManager == null || CheatEngineCore.TargetProcess == null)
            {
                throw new InvalidOperationException("No process is attached");
            }
            
            try
            {
                // Start with the base address
                IntPtr currentAddress = path.BaseAddress;
                
                // Follow each offset
                for (int i = 0; i < path.Offsets.Count; i++)
                {
                    // Read pointer value
                    if (i < path.Offsets.Count - 1)
                    {
                        // Read as pointer
                        if (IntPtr.Size == 4)
                        {
                            int value = CheatEngineCore.MemoryManager.ReadValue<int>(currentAddress);
                            currentAddress = new IntPtr(value);
                        }
                        else
                        {
                            long value = CheatEngineCore.MemoryManager.ReadValue<long>(currentAddress);
                            currentAddress = new IntPtr(value);
                        }
                    }
                    
                    // Add offset
                    currentAddress = new IntPtr(currentAddress.ToInt64() + path.Offsets[i]);
                }
                
                return currentAddress;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error resolving pointer path: {ex.Message}", LogLevel.Error);
                return IntPtr.Zero;
            }
        }
        
        /// <summary>
        /// Raises the ScanComplete event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnScanComplete(PointerScanCompleteEventArgs e)
        {
            ScanComplete?.Invoke(this, e);
        }
        
        /// <summary>
        /// Raises the ScanProgress event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnScanProgress(PointerScanProgressEventArgs e)
        {
            ScanProgress?.Invoke(this, e);
        }
    }
    
    /// <summary>
    /// Settings for a pointer scan
    /// </summary>
    public class PointerScanSettings
    {
        /// <summary>
        /// Gets or sets the target address
        /// </summary>
        public IntPtr TargetAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum pointer level
        /// </summary>
        public int MaxLevel { get; set; } = 3;
        
        /// <summary>
        /// Gets or sets the maximum offset value
        /// </summary>
        public int MaxOffset { get; set; } = 4096;
    }
    
    /// <summary>
    /// Information about a pointer
    /// </summary>
    public class PointerInfo
    {
        /// <summary>
        /// Gets or sets the address of the pointer
        /// </summary>
        public IntPtr Address { get; set; }
        
        /// <summary>
        /// Gets or sets the address that the pointer points to
        /// </summary>
        public IntPtr PointsTo { get; set; }
        
        /// <summary>
        /// Gets or sets the offset from the pointer value to the target address
        /// </summary>
        public int Offset { get; set; }
    }
    
    /// <summary>
    /// Represents a pointer path
    /// </summary>
    public class PointerPath
    {
        /// <summary>
        /// Gets or sets the base address
        /// </summary>
        public IntPtr BaseAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the offsets
        /// </summary>
        public List<int> Offsets { get; set; } = new List<int>();
        
        /// <summary>
        /// Gets or sets the target address
        /// </summary>
        public IntPtr TargetAddress { get; set; }
        
        /// <summary>
        /// Gets the final address (alias for TargetAddress for backward compatibility)
        /// </summary>
        public IntPtr FinalAddress => TargetAddress;
        
        /// <summary>
        /// Gets the level of the pointer path
        /// </summary>
        public int Level => Offsets.Count;
        
        /// <summary>
        /// Gets the base address as a hexadecimal string
        /// </summary>
        public string BaseAddressString => $"0x{BaseAddress.ToInt64():X}";
        
        /// <summary>
        /// Gets the offsets as a string
        /// </summary>
        public string OffsetsString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < Offsets.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append($"0x{Offsets[i]:X}");
                }
                return sb.ToString();
            }
        }
    }
    
    /// <summary>
    /// Event arguments for pointer scan complete event
    /// </summary>
    public class PointerScanCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the number of results
        /// </summary>
        public int ResultCount { get; }
        
        /// <summary>
        /// Gets the scan results
        /// </summary>
        public IReadOnlyList<PointerPath> Results { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PointerScanCompleteEventArgs"/> class
        /// </summary>
        /// <param name="resultCount">The number of results</param>
        /// <param name="results">The scan results</param>
        public PointerScanCompleteEventArgs(int resultCount, IReadOnlyList<PointerPath> results)
        {
            ResultCount = resultCount;
            Results = results;
        }
    }
    
    /// <summary>
    /// Event arguments for pointer scan progress event
    /// </summary>
    public class PointerScanProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the progress percentage
        /// </summary>
        public int ProgressPercentage { get; }
        
        /// <summary>
        /// Gets the number of pointers found so far
        /// </summary>
        public int PointersFound { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PointerScanProgressEventArgs"/> class
        /// </summary>
        /// <param name="progressPercentage">The progress percentage</param>
        public PointerScanProgressEventArgs(int progressPercentage)
        {
            ProgressPercentage = progressPercentage;
            PointersFound = 0;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PointerScanProgressEventArgs"/> class
        /// </summary>
        /// <param name="progressPercentage">The progress percentage</param>
        /// <param name="pointersFound">The number of pointers found</param>
        public PointerScanProgressEventArgs(int progressPercentage, int pointersFound)
        {
            ProgressPercentage = progressPercentage;
            PointersFound = pointersFound;
        }
    }
}
