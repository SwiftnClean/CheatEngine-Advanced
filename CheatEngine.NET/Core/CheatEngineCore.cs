using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheatEngine.NET.Memory;
using CheatEngine.NET.Debugger;
using CheatEngine.NET.Scanner;
using CheatEngine.NET.Utils;

namespace CheatEngine.NET.Core
{
    /// <summary>
    /// Core functionality of Cheat Engine .NET
    /// </summary>
    public static class CheatEngineCore
    {
        private static Process? _targetProcess;
        private static MemoryManager? _memoryManager;
        private static DebuggerManager? _debuggerManager;
        private static AddressListManager? _addressListManager;
        private static bool _isInitialized = false;
        
        /// <summary>
        /// Gets the currently attached process
        /// </summary>
        public static Process? TargetProcess => _targetProcess;
        
        /// <summary>
        /// Gets the memory manager instance
        /// </summary>
        public static MemoryManager? MemoryManager => _memoryManager;
        
        /// <summary>
        /// Gets the debugger manager instance
        /// </summary>
        public static DebuggerManager? DebuggerManager => _debuggerManager;
        
        /// <summary>
        /// Gets the address list manager instance
        /// </summary>
        public static AddressListManager? AddressListManager => _addressListManager;
        
        /// <summary>
        /// Gets a value indicating whether the core is initialized
        /// </summary>
        public static bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Initializes the Cheat Engine core
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
                return;
                
            // Initialize components
            _memoryManager = new MemoryManager();
            _debuggerManager = new DebuggerManager();
            _addressListManager = new AddressListManager();
            
            _isInitialized = true;
            
            Logger.Log("CheatEngineCore initialized successfully");
        }
        
        /// <summary>
        /// Attaches to a process by its ID
        /// </summary>
        /// <param name="processId">The ID of the process to attach to</param>
        /// <returns>True if the attachment was successful, false otherwise</returns>
        public static bool AttachToProcess(int processId)
        {
            if (!_isInitialized)
            {
                Logger.Log("CheatEngineCore not initialized", LogLevel.Error);
                return false;
            }
            
            try
            {
                // Get the process
                _targetProcess = Process.GetProcessById(processId);
                
                // Initialize memory manager with the target process
                if (_memoryManager != null)
                {
                    _memoryManager.Initialize(_targetProcess);
                }
                
                // Initialize debugger manager with the target process
                if (_debuggerManager != null)
                {
                    _debuggerManager.Initialize(_targetProcess);
                }
                
                Logger.Log($"Successfully attached to process {_targetProcess.ProcessName} (PID: {_targetProcess.Id})");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to attach to process with ID {processId}: {ex.Message}", LogLevel.Error);
                _targetProcess = null;
                return false;
            }
        }
        
        /// <summary>
        /// Detaches from the current process
        /// </summary>
        public static void DetachFromProcess()
        {
            if (_targetProcess != null)
            {
                // Clean up memory manager
                if (_memoryManager != null)
                {
                    _memoryManager.Cleanup();
                }
                
                // Clean up debugger manager
                if (_debuggerManager != null)
                {
                    _debuggerManager.Cleanup();
                }
                
                Logger.Log($"Detached from process {_targetProcess.ProcessName} (PID: {_targetProcess.Id})");
                _targetProcess = null;
            }
        }
        
        /// <summary>
        /// Shuts down the Cheat Engine core
        /// </summary>
        public static void Shutdown()
        {
            if (!_isInitialized)
                return;
                
            // Detach from the current process
            DetachFromProcess();
            
            // Clean up resources
            _memoryManager = null;
            _debuggerManager = null;
            
            _isInitialized = false;
            
            Logger.Log("CheatEngineCore shut down successfully");
        }
    }
}
