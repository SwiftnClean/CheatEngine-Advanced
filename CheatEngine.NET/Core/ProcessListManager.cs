using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Threading.Tasks;
using CheatEngine.NET.Native;
using CheatEngine.NET.Utils;

namespace CheatEngine.NET.Core
{
    /// <summary>
    /// Manages the list of processes for selection
    /// </summary>
    public class ProcessListManager
    {
        private readonly BindingList<ProcessInfo> _processes = new BindingList<ProcessInfo>();
        private readonly List<int> _hiddenProcessIds = new List<int>();
        private bool _showSystemProcesses = false;
        private bool _show64BitProcesses = true;
        private bool _show32BitProcesses = true;
        
        /// <summary>
        /// Event raised when the process list is refreshed
        /// </summary>
        public event EventHandler? ProcessListRefreshed;
        
        /// <summary>
        /// Gets the binding list of processes
        /// </summary>
        public BindingList<ProcessInfo> Processes => _processes;
        
        /// <summary>
        /// Gets or sets a value indicating whether system processes should be shown
        /// </summary>
        public bool ShowSystemProcesses
        {
            get => _showSystemProcesses;
            set
            {
                _showSystemProcesses = value;
                RefreshProcessList();
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether 64-bit processes should be shown
        /// </summary>
        public bool Show64BitProcesses
        {
            get => _show64BitProcesses;
            set
            {
                _show64BitProcesses = value;
                RefreshProcessList();
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether 32-bit processes should be shown
        /// </summary>
        public bool Show32BitProcesses
        {
            get => _show32BitProcesses;
            set
            {
                _show32BitProcesses = value;
                RefreshProcessList();
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessListManager"/> class
        /// </summary>
        public ProcessListManager()
        {
            // Add current process to hidden list
            _hiddenProcessIds.Add(Process.GetCurrentProcess().Id);
        }
        
        /// <summary>
        /// Refreshes the process list
        /// </summary>
        public void RefreshProcessList()
        {
            try
            {
                // Clear current list
                _processes.Clear();
                
                // Get all processes
                Process[] processes = Process.GetProcesses();
                
                // Filter and add processes
                foreach (Process process in processes)
                {
                    try
                    {
                        // Skip hidden processes
                        if (_hiddenProcessIds.Contains(process.Id))
                        {
                            continue;
                        }
                        
                        // Skip system processes if not showing them
                        if (!_showSystemProcesses && IsSystemProcess(process))
                        {
                            continue;
                        }
                        
                        // Check process architecture
                        bool is64Bit = false;
                        try
                        {
                            is64Bit = WinAPI.Is64BitProcess(process);
                        }
                        catch
                        {
                            // Skip processes that can't be queried
                            continue;
                        }
                        
                        // Skip based on architecture filter
                        if ((is64Bit && !_show64BitProcesses) || (!is64Bit && !_show32BitProcesses))
                        {
                            continue;
                        }
                        
                        // Get process info
                        string processName = process.ProcessName;
                        string windowTitle = GetProcessWindowTitle(process);
                        
                        // Create process info
                        ProcessInfo processInfo = new ProcessInfo
                        {
                            Id = process.Id,
                            Name = processName,
                            WindowTitle = windowTitle,
                            Is64Bit = is64Bit
                        };
                        
                        // Add to list
                        _processes.Add(processInfo);
                    }
                    catch
                    {
                        // Skip processes that can't be accessed
                    }
                }
                
                // Sort by name
                List<ProcessInfo> sortedList = _processes.OrderBy(p => p.Name).ToList();
                _processes.Clear();
                foreach (var process in sortedList)
                {
                    _processes.Add(process);
                }
                
                // Raise event
                OnProcessListRefreshed();
                
                Logger.Log($"Process list refreshed, found {_processes.Count} processes");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error refreshing process list: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Refreshes the process list asynchronously
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task RefreshProcessListAsync()
        {
            await Task.Run(() => RefreshProcessList());
        }
        
        /// <summary>
        /// Gets a process by its ID
        /// </summary>
        /// <param name="processId">The process ID</param>
        /// <returns>The process info, or null if not found</returns>
        public ProcessInfo? GetProcessById(int processId)
        {
            return _processes.FirstOrDefault(p => p.Id == processId);
        }
        
        /// <summary>
        /// Gets a process by its name
        /// </summary>
        /// <param name="processName">The process name</param>
        /// <returns>The process info, or null if not found</returns>
        public ProcessInfo? GetProcessByName(string processName)
        {
            return _processes.FirstOrDefault(p => p.Name.Equals(processName, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Adds a process ID to the hidden list
        /// </summary>
        /// <param name="processId">The process ID to hide</param>
        public void HideProcess(int processId)
        {
            if (!_hiddenProcessIds.Contains(processId))
            {
                _hiddenProcessIds.Add(processId);
                RefreshProcessList();
            }
        }
        
        /// <summary>
        /// Removes a process ID from the hidden list
        /// </summary>
        /// <param name="processId">The process ID to show</param>
        public void ShowProcess(int processId)
        {
            if (_hiddenProcessIds.Contains(processId))
            {
                _hiddenProcessIds.Remove(processId);
                RefreshProcessList();
            }
        }
        
        /// <summary>
        /// Checks if a process is a system process
        /// </summary>
        /// <param name="process">The process to check</param>
        /// <returns>True if the process is a system process, false otherwise</returns>
        private bool IsSystemProcess(Process process)
        {
            try
            {
                // Check if process is running as SYSTEM
                string user = GetProcessOwner(process.Id);
                if (user.Contains("SYSTEM", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                
                // Check common system process names
                string[] systemProcessNames = new[]
                {
                    "svchost", "csrss", "smss", "wininit", "winlogon", "services", "lsass", "spoolsv", "dwm"
                };
                
                return systemProcessNames.Contains(process.ProcessName.ToLower());
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Gets the window title of a process
        /// </summary>
        /// <param name="process">The process</param>
        /// <returns>The window title, or an empty string if not found</returns>
        private string GetProcessWindowTitle(Process process)
        {
            try
            {
                return process.MainWindowTitle;
            }
            catch
            {
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Gets the owner of a process
        /// </summary>
        /// <param name="processId">The process ID</param>
        /// <returns>The process owner, or an empty string if not found</returns>
        private string GetProcessOwner(int processId)
        {
            try
            {
                string query = $"SELECT * FROM Win32_Process WHERE ProcessId = {processId}";
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string[] argList = new string[] { string.Empty, string.Empty };
                        int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                        
                        if (returnVal == 0)
                        {
                            return argList[0];
                        }
                    }
                }
                
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Checks if the current process has administrator privileges
        /// </summary>
        /// <returns>True if the process has administrator privileges, false otherwise</returns>
        public static bool IsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Raises the ProcessListRefreshed event
        /// </summary>
        protected virtual void OnProcessListRefreshed()
        {
            ProcessListRefreshed?.Invoke(this, EventArgs.Empty);
        }
    }
    
    /// <summary>
    /// Represents information about a process
    /// </summary>
    public class ProcessInfo
    {
        /// <summary>
        /// Gets or sets the process ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the process name
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the window title
        /// </summary>
        public string WindowTitle { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets a value indicating whether the process is 64-bit
        /// </summary>
        public bool Is64Bit { get; set; }
        
        /// <summary>
        /// Gets the display name of the process
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(WindowTitle))
                {
                    return $"{Name} ({Id})";
                }
                else
                {
                    return $"{Name} - {WindowTitle} ({Id})";
                }
            }
        }
        
        /// <summary>
        /// Gets the architecture string
        /// </summary>
        public string Architecture => Is64Bit ? "64-bit" : "32-bit";
    }
}
