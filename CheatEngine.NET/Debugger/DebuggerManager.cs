using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CheatEngine.NET.Memory;
using CheatEngine.NET.Native;
using CheatEngine.NET.Utils;

namespace CheatEngine.NET.Debugger
{
    /// <summary>
    /// Manages debugging operations for the target process
    /// </summary>
    public class DebuggerManager
    {
        private Process? _process;
        private IntPtr _processHandle = IntPtr.Zero;
        private bool _isInitialized = false;
        private bool _isDebugging = false;
        private CancellationTokenSource? _debugCancellationTokenSource;
        private readonly List<Breakpoint> _breakpoints = new List<Breakpoint>();
        
        /// <summary>
        /// Event raised when a breakpoint is hit
        /// </summary>
        public event EventHandler<BreakpointEventArgs>? BreakpointHit;
        
        /// <summary>
        /// Event raised when debugging starts
        /// </summary>
        public event EventHandler? DebuggingStarted;
        
        /// <summary>
        /// Event raised when debugging stops
        /// </summary>
        public event EventHandler? DebuggingStopped;
        
        /// <summary>
        /// Gets a value indicating whether the debugger is initialized
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Gets a value indicating whether the debugger is currently debugging
        /// </summary>
        public bool IsDebugging => _isDebugging;
        
        /// <summary>
        /// Gets the target process
        /// </summary>
        public Process? Process => _process;
        
        /// <summary>
        /// Gets the list of breakpoints
        /// </summary>
        public IReadOnlyList<Breakpoint> Breakpoints => _breakpoints.AsReadOnly();
        
        /// <summary>
        /// Initializes the debugger manager with a target process
        /// </summary>
        /// <param name="process">The target process</param>
        public void Initialize(Process process)
        {
            if (_isInitialized)
            {
                Cleanup();
            }
            
            _process = process;
            
            // Open process with all access rights
            _processHandle = WinAPI.OpenProcess(
                WinAPI.ProcessAccessFlags.All,
                false,
                _process.Id);
                
            if (_processHandle == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                Logger.Log($"Failed to open process handle for debugging. Error code: {error}", LogLevel.Error);
                throw new Exception($"Failed to open process handle for debugging. Error code: {error}");
            }
            
            _isInitialized = true;
            Logger.Log($"Debugger manager initialized for process {_process.ProcessName} (PID: {_process.Id})");
        }
        
        /// <summary>
        /// Cleans up resources used by the debugger manager
        /// </summary>
        public void Cleanup()
        {
            // Stop debugging if active
            if (_isDebugging)
            {
                StopDebugging();
            }
            
            // Remove all breakpoints
            RemoveAllBreakpoints();
            
            if (_processHandle != IntPtr.Zero)
            {
                WinAPI.CloseHandle(_processHandle);
                _processHandle = IntPtr.Zero;
            }
            
            _process = null;
            _isInitialized = false;
            
            Logger.Log("Debugger manager cleaned up");
        }
        
        /// <summary>
        /// Starts debugging the target process
        /// </summary>
        public void StartDebugging()
        {
            CheckInitialized();
            
            if (_isDebugging)
            {
                return;
            }
            
            _isDebugging = true;
            _debugCancellationTokenSource = new CancellationTokenSource();
            
            // Start debugging in background
            Task.Run(() => DebugLoop(_debugCancellationTokenSource.Token));
            
            OnDebuggingStarted();
            Logger.Log("Debugging started");
        }
        
        /// <summary>
        /// Stops debugging the target process
        /// </summary>
        public void StopDebugging()
        {
            if (!_isDebugging || _debugCancellationTokenSource == null)
            {
                return;
            }
            
            _debugCancellationTokenSource.Cancel();
            _isDebugging = false;
            
            OnDebuggingStopped();
            Logger.Log("Debugging stopped");
        }
        
        /// <summary>
        /// Adds a breakpoint at the specified address
        /// </summary>
        /// <param name="address">The memory address</param>
        /// <param name="description">The breakpoint description</param>
        /// <returns>The created breakpoint</returns>
        public Breakpoint AddBreakpoint(IntPtr address, string description = "")
        {
            CheckInitialized();
            
            // Check if breakpoint already exists
            foreach (var bp in _breakpoints)
            {
                if (bp.Address == address)
                {
                    throw new Exception($"Breakpoint already exists at address 0x{address.ToInt64():X}");
                }
            }
            
            // Create breakpoint
            Breakpoint breakpoint = new Breakpoint
            {
                Address = address,
                Description = description,
                IsEnabled = true
            };
            
            // Set breakpoint
            if (!SetBreakpoint(breakpoint))
            {
                throw new Exception($"Failed to set breakpoint at address 0x{address.ToInt64():X}");
            }
            
            // Add to list
            _breakpoints.Add(breakpoint);
            
            Logger.Log($"Breakpoint added at address 0x{address.ToInt64():X}");
            return breakpoint;
        }
        
        /// <summary>
        /// Removes a breakpoint
        /// </summary>
        /// <param name="breakpoint">The breakpoint to remove</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool RemoveBreakpoint(Breakpoint breakpoint)
        {
            CheckInitialized();
            
            if (breakpoint == null)
            {
                Logger.Log("Cannot remove null breakpoint", LogLevel.Error);
                return false;
            }
            
            // Remove breakpoint
            if (breakpoint.IsEnabled)
            {
                if (!RemoveBreakpointImpl(breakpoint))
                {
                    Logger.Log($"Failed to remove breakpoint at address 0x{breakpoint.Address.ToInt64():X}", LogLevel.Error);
                    return false;
                }
            }
            
            // Remove from list
            _breakpoints.Remove(breakpoint);
            
            Logger.Log($"Breakpoint removed from address 0x{breakpoint.Address.ToInt64():X}");
            return true;
        }
        
        /// <summary>
        /// Removes all breakpoints
        /// </summary>
        public void RemoveAllBreakpoints()
        {
            CheckInitialized();
            
            // Remove all breakpoints
            foreach (var bp in _breakpoints)
            {
                if (bp.IsEnabled)
                {
                    RemoveBreakpoint(bp);
                }
            }
            
            // Clear list
            _breakpoints.Clear();
            
            Logger.Log("All breakpoints removed");
        }
        
        /// <summary>
        /// Enables a breakpoint
        /// </summary>
        /// <param name="breakpoint">The breakpoint to disable</param>
        public void DisableBreakpoint(Breakpoint breakpoint)
        {
            CheckInitialized();
            
            if (!breakpoint.IsEnabled)
            {
                return;
            }
            
            // Remove breakpoint
            if (!RemoveBreakpoint(breakpoint))
            {
                throw new Exception($"Failed to disable breakpoint at address 0x{breakpoint.Address.ToInt64():X}");
            }
            
            breakpoint.IsEnabled = false;
            
            Logger.Log($"Breakpoint disabled at address 0x{breakpoint.Address.ToInt64():X}");
        }
        
        /// <summary>
        /// Sets a breakpoint
        /// </summary>
        /// <param name="breakpoint">The breakpoint to set</param>
        /// <returns>True if successful, false otherwise</returns>
        private bool SetBreakpoint(Breakpoint breakpoint)
        {
            try
            {
                // Read original byte
                byte[] buffer = new byte[1];
                int bytesRead = 0;
                
                bool success = WinAPI.ReadProcessMemory(
                    _processHandle,
                    breakpoint.Address,
                    buffer,
                    1,
                    ref bytesRead);
                    
                if (!success || bytesRead != 1)
                {
                    int error = Marshal.GetLastWin32Error();
                    Logger.Log($"Failed to read memory at breakpoint address 0x{breakpoint.Address.ToInt64():X}. Error code: {error}", LogLevel.Error);
                    return false;
                }
                
                // Save original byte
                breakpoint.OriginalByte = buffer[0];
                
                // Write INT3 instruction (0xCC)
                buffer[0] = 0xCC;
                int bytesWritten = 0;
                
                success = WinAPI.WriteProcessMemory(
                    _processHandle,
                    breakpoint.Address,
                    buffer,
                    1,
                    ref bytesWritten);
                    
                if (!success || bytesWritten != 1)
                {
                    int error = Marshal.GetLastWin32Error();
                    Logger.Log($"Failed to write INT3 instruction at breakpoint address 0x{breakpoint.Address.ToInt64():X}. Error code: {error}", LogLevel.Error);
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error setting breakpoint: {ex.Message}", LogLevel.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Removes a breakpoint implementation
        /// </summary>
        /// <param name="breakpoint">The breakpoint to remove</param>
        /// <returns>True if successful, false otherwise</returns>
        private bool RemoveBreakpointImpl(Breakpoint breakpoint)
        {
            try
            {
                // Restore original byte
                byte[] buffer = new byte[1];
                buffer[0] = breakpoint.OriginalByte;
                int bytesWritten = 0;
                
                bool success = WinAPI.WriteProcessMemory(
                    _processHandle,
                    breakpoint.Address,
                    buffer,
                    1,
                    ref bytesWritten);
                    
                if (!success || bytesWritten != 1)
                {
                    int error = Marshal.GetLastWin32Error();
                    Logger.Log($"Failed to restore original byte at breakpoint address 0x{breakpoint.Address.ToInt64():X}. Error code: {error}", LogLevel.Error);
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error removing breakpoint: {ex.Message}", LogLevel.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Debug loop
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        private void DebugLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Check for breakpoints
                    // This is a simplified implementation
                    // A real implementation would use debugging APIs
                    
                    // Sleep to avoid high CPU usage
                    Thread.Sleep(100);
                }
            }
            catch (OperationCanceledException)
            {
                // Debugging was cancelled
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in debug loop: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Checks if the debugger manager is initialized
        /// </summary>
        private void CheckInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Debugger manager is not initialized");
            }
        }
        
        /// <summary>
        /// Raises the BreakpointHit event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnBreakpointHit(BreakpointEventArgs e)
        {
            BreakpointHit?.Invoke(this, e);
        }
        
        /// <summary>
        /// Raises the DebuggingStarted event
        /// </summary>
        protected virtual void OnDebuggingStarted()
        {
            DebuggingStarted?.Invoke(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// Raises the DebuggingStopped event
        /// </summary>
        protected virtual void OnDebuggingStopped()
        {
            DebuggingStopped?.Invoke(this, EventArgs.Empty);
        }
    }
    
    /// <summary>
    /// Represents a breakpoint
    /// </summary>
    public class Breakpoint
    {
        /// <summary>
        /// Gets or sets the memory address
        /// </summary>
        public IntPtr Address { get; set; }
        
        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets a value indicating whether the breakpoint is enabled
        /// </summary>
        public bool IsEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets the original byte at the breakpoint address
        /// </summary>
        public byte OriginalByte { get; set; }
    }
    
    /// <summary>
    /// Event arguments for breakpoint hit event
    /// </summary>
    public class BreakpointEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the breakpoint that was hit
        /// </summary>
        public Breakpoint Breakpoint { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BreakpointEventArgs"/> class
        /// </summary>
        /// <param name="breakpoint">The breakpoint that was hit</param>
        public BreakpointEventArgs(Breakpoint breakpoint)
        {
            Breakpoint = breakpoint;
        }
    }
}
