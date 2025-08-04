using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CheatEngine.NET.Core;
using CheatEngine.NET.Memory;
using CheatEngine.NET.Utils;
using NLua;

namespace CheatEngine.NET.Scripting
{
    /// <summary>
    /// Provides Lua scripting functionality
    /// </summary>
    public class LuaEngine
    {
        private Lua _lua;
        private bool _isInitialized = false;
        private readonly Dictionary<string, object> _registeredObjects = new Dictionary<string, object>();
        private CancellationTokenSource? _scriptCancellationTokenSource;
        
        /// <summary>
        /// Event raised when a script is executed
        /// </summary>
        public event EventHandler<LuaScriptEventArgs>? ScriptExecuted;
        
        /// <summary>
        /// Event raised when a script execution is completed (alias for ScriptExecuted for backward compatibility)
        /// </summary>
        public event EventHandler<LuaScriptEventArgs>? ScriptExecutionCompleted
        {
            add { ScriptExecuted += value; }
            remove { ScriptExecuted -= value; }
        }
        
        /// <summary>
        /// Event raised when a script outputs text
        /// </summary>
        public event EventHandler<LuaOutputEventArgs>? ScriptOutput;
        
        /// <summary>
        /// Event raised when a script encounters an error (alias for ScriptOutput for backward compatibility)
        /// </summary>
        public event EventHandler<LuaOutputEventArgs>? ScriptError
        {
            add { ScriptOutput += value; }
            remove { ScriptOutput -= value; }
        }
        
        /// <summary>
        /// Gets a value indicating whether the Lua engine is initialized
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="LuaEngine"/> class
        /// </summary>
        public LuaEngine()
        {
            _lua = new Lua();
        }
        
        /// <summary>
        /// Initializes the Lua engine
        /// </summary>
        public void Initialize()
        {
            try
            {
                if (_isInitialized)
                {
                    return;
                }
                
                // Create a new Lua state
                _lua = new Lua();
                
                // Register standard libraries
                _lua.LoadCLRPackage();
                
                // Register custom functions
                RegisterStandardFunctions();
                
                // Register CheatEngine objects
                RegisterCheatEngineObjects();
                
                _isInitialized = true;
                
                Logger.Log("Lua engine initialized");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error initializing Lua engine: {ex.Message}", LogLevel.Error);
                _isInitialized = false;
            }
        }
        
        /// <summary>
        /// Shuts down the Lua engine
        /// </summary>
        public void Shutdown()
        {
            try
            {
                if (!_isInitialized)
                {
                    return;
                }
                
                _lua.Dispose();
                _isInitialized = false;
                
                Logger.Log("Lua engine shut down");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error shutting down Lua engine: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Executes a Lua script
        /// </summary>
        /// <param name="script">The script to execute</param>
        /// <returns>The result of the script execution</returns>
        public object? ExecuteScript(string script)
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            
            try
            {
                // Execute the script
                object[] results = _lua.DoString(script);
                
                // Raise event
                OnScriptExecuted(new LuaScriptEventArgs(script, true, null));
                
                // Return the first result if any
                return results.Length > 0 ? results[0] : null;
            }
            catch (Exception ex)
            {
                // Log error
                Logger.Log($"Error executing Lua script: {ex.Message}", LogLevel.Error);
                
                // Raise event
                OnScriptExecuted(new LuaScriptEventArgs(script, false, ex.Message));
                
                return null;
            }
        }
        
        /// <summary>
        /// Executes a Lua script asynchronously
        /// </summary>
        /// <param name="script">The script to execute</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task<object?> ExecuteScriptAsync(string script)
        {
            // Cancel any existing script
            StopScript();
            
            // Create new cancellation token source
            _scriptCancellationTokenSource = new CancellationTokenSource();
            
            try
            {
                return await Task.Run(() => ExecuteScript(script), _scriptCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Logger.Log("Script execution was cancelled", LogLevel.Info);
                OnScriptExecuted(new LuaScriptEventArgs(script, false, "Script execution was cancelled"));
                return null;
            }
        }
        
        /// <summary>
        /// Stops the currently executing script
        /// </summary>
        public void StopScript()
        {
            if (_scriptCancellationTokenSource != null && !_scriptCancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    _scriptCancellationTokenSource.Cancel();
                    Logger.Log("Script execution cancelled", LogLevel.Info);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error stopping script: {ex.Message}", LogLevel.Error);
                }
                finally
                {
                    _scriptCancellationTokenSource.Dispose();
                    _scriptCancellationTokenSource = null;
                }
            }
        }
        
        /// <summary>
        /// Executes a Lua script from a file
        /// </summary>
        /// <param name="filePath">The path to the script file</param>
        /// <returns>The result of the script execution</returns>
        public object? ExecuteFile(string filePath)
        {
            try
            {
                // Read the script file
                string script = File.ReadAllText(filePath);
                
                // Execute the script
                return ExecuteScript(script);
            }
            catch (Exception ex)
            {
                // Log error
                Logger.Log($"Error executing Lua file '{filePath}': {ex.Message}", LogLevel.Error);
                
                // Raise event
                OnScriptExecuted(new LuaScriptEventArgs(filePath, false, ex.Message));
                
                return null;
            }
        }
        
        /// <summary>
        /// Registers an object with the Lua engine
        /// </summary>
        /// <param name="name">The name to register the object as</param>
        /// <param name="obj">The object to register</param>
        public void RegisterObject(string name, object obj)
        {
            // Don't call Initialize() here to avoid infinite recursion
            // since RegisterCheatEngineObjects() calls this method
            
            try
            {
                if (_isInitialized) // Only register if already initialized
                {
                    _lua[name] = obj;
                    _registeredObjects[name] = obj;
                    
                    Logger.Log($"Registered object '{name}' with Lua engine");
                }
                else
                {
                    // Just store the object for later registration
                    _registeredObjects[name] = obj;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error registering object '{name}': {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Gets a registered object
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <returns>The object, or null if not found</returns>
        public object? GetRegisteredObject(string name)
        {
            if (_registeredObjects.TryGetValue(name, out object? obj))
            {
                return obj;
            }
            
            return null;
        }
        
        /// <summary>
        /// Registers standard Lua functions
        /// </summary>
        private void RegisterStandardFunctions()
        {
            // Register print function
            _lua.RegisterFunction("print", this, GetType().GetMethod("LuaPrint"));
            
            // Register sleep function
            _lua.RegisterFunction("sleep", this, GetType().GetMethod("LuaSleep"));
            
            // Register message box function
            _lua.RegisterFunction("messageBox", this, GetType().GetMethod("LuaMessageBox"));
            
            // Register input box function
            _lua.RegisterFunction("inputBox", this, GetType().GetMethod("LuaInputBox"));
        }
        
        /// <summary>
        /// Registers CheatEngine objects with the Lua engine
        /// </summary>
        private void RegisterCheatEngineObjects()
        {
            // Only register objects if the engine is initialized
            // to prevent infinite recursion
            if (!_isInitialized)
            {
                return;
            }
            
            // Register memory manager
            if (CheatEngineCore.MemoryManager != null)
            {
                _lua["memoryManager"] = CheatEngineCore.MemoryManager;
                _registeredObjects["memoryManager"] = CheatEngineCore.MemoryManager;
            }
            
            // Register debugger manager
            if (CheatEngineCore.DebuggerManager != null)
            {
                _lua["debuggerManager"] = CheatEngineCore.DebuggerManager;
                _registeredObjects["debuggerManager"] = CheatEngineCore.DebuggerManager;
            }
            
            // Register address list manager
            if (CheatEngineCore.AddressListManager != null)
            {
                _lua["addressListManager"] = CheatEngineCore.AddressListManager;
                _registeredObjects["addressListManager"] = CheatEngineCore.AddressListManager;
            }
            
            // Register helper objects
            _lua["memoryHelper"] = new LuaMemoryHelper();
            _registeredObjects["memoryHelper"] = new LuaMemoryHelper();
            
            Logger.Log("Registered CheatEngine objects with Lua engine");
        }
        
        /// <summary>
        /// Lua print function
        /// </summary>
        /// <param name="text">The text to print</param>
        public void LuaPrint(object text)
        {
            string message = text?.ToString() ?? string.Empty;
            
            // Log the message
            Logger.Log($"[Lua] {message}");
            
            // Raise event
            OnScriptOutput(new LuaOutputEventArgs(message));
        }
        
        /// <summary>
        /// Lua sleep function
        /// </summary>
        /// <param name="milliseconds">The number of milliseconds to sleep</param>
        public void LuaSleep(int milliseconds)
        {
            System.Threading.Thread.Sleep(milliseconds);
        }
        
        /// <summary>
        /// Lua message box function
        /// </summary>
        /// <param name="text">The message text</param>
        /// <param name="caption">The message caption</param>
        /// <returns>The dialog result</returns>
        public int LuaMessageBox(string text, string caption = "Lua Script")
        {
            // This would normally show a message box, but we'll just log it for now
            Logger.Log($"[Lua MessageBox] {caption}: {text}");
            
            // Raise event
            OnScriptOutput(new LuaOutputEventArgs($"MessageBox: {caption} - {text}"));
            
            return 1; // OK
        }
        
        /// <summary>
        /// Lua input box function
        /// </summary>
        /// <param name="prompt">The prompt text</param>
        /// <param name="caption">The input caption</param>
        /// <param name="defaultValue">The default value</param>
        /// <returns>The input value</returns>
        public string LuaInputBox(string prompt, string caption = "Lua Script", string defaultValue = "")
        {
            // This would normally show an input box, but we'll just log it for now
            Logger.Log($"[Lua InputBox] {caption}: {prompt} (Default: {defaultValue})");
            
            // Raise event
            OnScriptOutput(new LuaOutputEventArgs($"InputBox: {caption} - {prompt}"));
            
            return defaultValue;
        }
        
        /// <summary>
        /// Raises the ScriptExecuted event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnScriptExecuted(LuaScriptEventArgs e)
        {
            ScriptExecuted?.Invoke(this, e);
        }
        
        /// <summary>
        /// Raises the ScriptOutput event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnScriptOutput(LuaOutputEventArgs e)
        {
            ScriptOutput?.Invoke(this, e);
        }
    }
    
    /// <summary>
    /// Helper class for Lua memory operations
    /// </summary>
    public class LuaMemoryHelper
    {
        /// <summary>
        /// Reads a byte from memory
        /// </summary>
        /// <param name="address">The address to read from</param>
        /// <returns>The byte value</returns>
        public byte ReadByte(long address)
        {
            if (CheatEngineCore.MemoryManager == null)
            {
                throw new InvalidOperationException("No process is attached");
            }
            
            return CheatEngineCore.MemoryManager.ReadValue<byte>(new IntPtr(address));
        }
        
        /// <summary>
        /// Reads a short from memory
        /// </summary>
        /// <param name="address">The address to read from</param>
        /// <returns>The short value</returns>
        public short ReadShort(long address)
        {
            if (CheatEngineCore.MemoryManager == null)
            {
                throw new InvalidOperationException("No process is attached");
            }
            
            return CheatEngineCore.MemoryManager.ReadValue<short>(new IntPtr(address));
        }
        
        /// <summary>
        /// Reads an integer from memory
        /// </summary>
        /// <param name="address">The address to read from</param>
        /// <returns>The integer value</returns>
        public int ReadInteger(long address)
        {
            if (CheatEngineCore.MemoryManager == null)
            {
                throw new InvalidOperationException("No process is attached");
            }
            
            return CheatEngineCore.MemoryManager.ReadValue<int>(new IntPtr(address));
        }
        
        /// <summary>
        /// Reads a long from memory
        /// </summary>
        /// <param name="address">The address to read from</param>
        /// <returns>The long value</returns>
        public long ReadLong(long address)
        {
            if (CheatEngineCore.MemoryManager == null)
            {
                throw new InvalidOperationException("No process is attached");
            }
            
            return CheatEngineCore.MemoryManager.ReadValue<long>(new IntPtr(address));
        }
        
        /// <summary>
        /// Reads a float from memory
        /// </summary>
        /// <param name="address">The address to read from</param>
        /// <returns>The float value</returns>
        public float ReadFloat(long address)
        {
            if (CheatEngineCore.MemoryManager == null)
            {
                throw new InvalidOperationException("No process is attached");
            }
            
            return CheatEngineCore.MemoryManager.ReadValue<float>(new IntPtr(address));
        }
        
        /// <summary>
        /// Reads a double from memory
        /// </summary>
        /// <param name="address">The address to read from</param>
        /// <returns>The double value</returns>
        public double ReadDouble(long address)
        {
            if (CheatEngineCore.MemoryManager == null)
            {
                throw new InvalidOperationException("No process is attached");
            }
            
            return CheatEngineCore.MemoryManager.ReadValue<double>(new IntPtr(address));
        }
        
        /// <summary>
        /// Writes a byte to memory
        /// </summary>
        /// <param name="address">The address to write to</param>
        /// <param name="value">The value to write</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool WriteByte(long address, byte value)
        {
            if (CheatEngineCore.MemoryManager == null)
            {
                throw new InvalidOperationException("No process is attached");
            }
            
            return CheatEngineCore.MemoryManager.WriteValue(new IntPtr(address), value);
        }
        
        /// <summary>
        /// Writes a short to memory
        /// </summary>
        /// <param name="address">The address to write to</param>
        /// <param name="value">The value to write</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool WriteShort(long address, short value)
        {
            if (CheatEngineCore.MemoryManager == null)
            {
                throw new InvalidOperationException("No process is attached");
            }
            
            return CheatEngineCore.MemoryManager.WriteValue(new IntPtr(address), value);
        }
        
        /// <summary>
        /// Writes an integer to memory
        /// </summary>
        /// <param name="address">The address to write to</param>
        /// <param name="value">The value to write</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool WriteInteger(long address, int value)
        {
            if (CheatEngineCore.MemoryManager == null)
            {
                throw new InvalidOperationException("No process is attached");
            }
            
            return CheatEngineCore.MemoryManager.WriteValue(new IntPtr(address), value);
        }
        
        /// <summary>
        /// Writes a long to memory
        /// </summary>
        /// <param name="address">The address to write to</param>
        /// <param name="value">The value to write</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool WriteLong(long address, long value)
        {
            if (CheatEngineCore.MemoryManager == null)
            {
                throw new InvalidOperationException("No process is attached");
            }
            
            return CheatEngineCore.MemoryManager.WriteValue(new IntPtr(address), value);
        }
        
        /// <summary>
        /// Writes a float to memory
        /// </summary>
        /// <param name="address">The address to write to</param>
        /// <param name="value">The value to write</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool WriteFloat(long address, float value)
        {
            if (CheatEngineCore.MemoryManager == null)
            {
                throw new InvalidOperationException("No process is attached");
            }
            
            return CheatEngineCore.MemoryManager.WriteValue(new IntPtr(address), value);
        }
        
        /// <summary>
        /// Writes a double to memory
        /// </summary>
        /// <param name="address">The address to write to</param>
        /// <param name="value">The value to write</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool WriteDouble(long address, double value)
        {
            if (CheatEngineCore.MemoryManager == null)
            {
                throw new InvalidOperationException("No process is attached");
            }
            
            return CheatEngineCore.MemoryManager.WriteValue(new IntPtr(address), value);
        }
    }
    
    /// <summary>
    /// Event arguments for Lua script execution
    /// </summary>
    public class LuaScriptEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the script that was executed
        /// </summary>
        public string Script { get; }
        
        /// <summary>
        /// Gets a value indicating whether the script executed successfully
        /// </summary>
        public bool Success { get; }
        
        /// <summary>
        /// Gets the error message if the script failed
        /// </summary>
        public string? ErrorMessage { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="LuaScriptEventArgs"/> class
        /// </summary>
        /// <param name="script">The script</param>
        /// <param name="success">Whether the script executed successfully</param>
        /// <param name="errorMessage">The error message if the script failed</param>
        public LuaScriptEventArgs(string script, bool success, string? errorMessage)
        {
            Script = script;
            Success = success;
            ErrorMessage = errorMessage;
        }
    }
    
    /// <summary>
    /// Event arguments for Lua script output
    /// </summary>
    public class LuaOutputEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the output text
        /// </summary>
        public string Text { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="LuaOutputEventArgs"/> class
        /// </summary>
        /// <param name="text">The output text</param>
        public LuaOutputEventArgs(string text)
        {
            Text = text;
        }
    }
}
