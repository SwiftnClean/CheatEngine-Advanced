using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
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
        // Process collections
        private readonly BindingList<ProcessInfo> _processes = new BindingList<ProcessInfo>();
        private readonly BindingList<ProcessInfo> _applications = new BindingList<ProcessInfo>();
        private readonly BindingList<WindowInfo> _windows = new BindingList<WindowInfo>();
        private readonly List<int> _hiddenProcessIds = new List<int>();
        
        // Process tree structure
        private readonly List<ProcessInfo> _rootProcesses = new List<ProcessInfo>();
        private readonly Dictionary<int, ProcessInfo> _processesById = new Dictionary<int, ProcessInfo>();
        private readonly ConcurrentDictionary<int, int> _parentProcessIdCache = new ConcurrentDictionary<int, int>();
        
        // Process icon cache
        private readonly Dictionary<string, string> _processIconKeys = new Dictionary<string, string>();
        
        // Filter settings
        private bool _showSystemProcesses = true; // Always show system processes
        private bool _show64BitProcesses = true;
        private bool _show32BitProcesses = true;
        private string _filterText = string.Empty;
        
        // Cancellation token source for async operations
        private CancellationTokenSource? _cancellationTokenSource;
        
        /// <summary>
        /// Event raised when the process list is refreshed
        /// </summary>
        public event EventHandler? ProcessListRefreshed;
        
        /// <summary>
        /// Gets the binding list of all processes
        /// </summary>
        public BindingList<ProcessInfo> Processes => _processes;
        
        /// <summary>
        /// Gets the binding list of applications (processes with windows)
        /// </summary>
        public BindingList<ProcessInfo> Applications => _applications;
        
        /// <summary>
        /// Gets the binding list of windows
        /// </summary>
        public BindingList<WindowInfo> Windows => _windows;
        
        /// <summary>
        /// Gets a value indicating whether system processes should be shown (always true)
        /// </summary>
        public bool ShowSystemProcesses
        {
            get => true; // Always return true
            set
            {
                // No-op, we always show system processes
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
        /// Gets the icon key for a process
        /// </summary>
        /// <param name="processId">The process ID</param>
        /// <param name="processName">The process name</param>
        /// <returns>The icon key</returns>
        private string GetProcessIconKey(int processId, string processName)
        {
            try
            {
                // Use process name as the key
                string key = processName.ToLowerInvariant();
                
                // If we don't have this icon yet, try to extract it
                if (!_processIconKeys.ContainsKey(key))
                {
                    try
                    {
                        // Try to get the icon from the process module
                        string? executablePath = GetFullPathFromProcessName(processName);
                        if (!string.IsNullOrEmpty(executablePath) && File.Exists(executablePath))
                        {
                            Icon? icon = Icon.ExtractAssociatedIcon(executablePath);
                            if (icon != null)
                            {
                                _processIconKeys[key] = key;
                                return key;
                            }
                        }
                    }
                    catch
                    {
                        // Fallback to default icon if extraction fails
                        _processIconKeys[key] = "default";
                        return "default";
                    }
                }
                
                return _processIconKeys[key];
            }
            catch
            {
                _processIconKeys[processName.ToLowerInvariant()] = "default";
                return "default";
            }
        }
        
        /// <summary>
        /// Gets the full path from a process name
        /// </summary>
        /// <param name="processName">The process name</param>
        /// <returns>The full path</returns>
        private string? GetFullPathFromProcessName(string processName)
        {
            try
            {
                // Try to get the main module path
                try
                {
                    Process? process = Process.GetProcessesByName(processName).FirstOrDefault();
                    if (process != null)
                    {
                        return process.MainModule?.FileName;
                    }
                }
                catch
                {
                    // For system processes, this might fail due to access restrictions
                }
                
                // Try WMI as fallback
                try
                {
                    using (var searcher = new ManagementObjectSearcher($"SELECT ExecutablePath FROM Win32_Process WHERE Name = '{processName}'"))
                    {
                        foreach (var obj in searcher.Get())
                        {
                            return obj["ExecutablePath"]?.ToString();
                        }
                    }
                }
                catch
                {
                    // WMI might fail too
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Refreshes the process list
        /// </summary>
        public void RefreshProcessList()
        {
            try
            {
                // Cancel any ongoing refresh operation
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                
                // Clear current lists and dictionaries
                _processes.Clear();
                _applications.Clear();
                _windows.Clear();
                _rootProcesses.Clear();
                _processesById.Clear();
                
                // Dictionary to track windows by process ID
                Dictionary<int, List<string>> windowsByProcessId = new Dictionary<int, List<string>>();
                
                // Get all processes
                Process[] processes = Process.GetProcesses();
                Logger.Log($"Found {processes.Length} total processes", LogLevel.Error);
                
                // First pass: Create process info objects and add to dictionary
                foreach (Process process in processes)
                {
                    try
                    {
                        // Skip hidden processes
                        if (_hiddenProcessIds.Contains(process.Id))
                        {
                            continue;
                        }
                        
                        // Skip system processes if not showing them (although we always show them now)
                        // Always include system processes as per requirement
                        // System process check removed - we want to show all processes
                        
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
                        
                        // Get basic process info
                        string processName = process.ProcessName;
                        string windowTitle = GetProcessWindowTitle(process);
                        
                        // Apply text filter if specified
                        if (!string.IsNullOrEmpty(_filterText))
                        {
                            bool matchesFilter = 
                                processName.Contains(_filterText, StringComparison.OrdinalIgnoreCase) ||
                                windowTitle.Contains(_filterText, StringComparison.OrdinalIgnoreCase) ||
                                process.Id.ToString().Contains(_filterText);
                                
                            if (!matchesFilter)
                            {
                                continue;
                            }
                        }
                        
                        // Get parent process ID (using cached value if available)
                        int parentPid = GetParentProcessId(process.Id);
                        
                        // Create enhanced process info object
                        ProcessInfo processInfo = new ProcessInfo
                        {
                            Id = process.Id,
                            Name = processName,
                            WindowTitle = windowTitle,
                            Is64Bit = is64Bit,
                            ParentProcessId = parentPid,
                            IconKey = processName.ToLowerInvariant()
                        };
                        
                        // Add additional process details if possible
                        try
                        {
                            processInfo.StartTime = process.StartTime;
                            processInfo.PrivateMemorySize = process.PrivateMemorySize64;
                            processInfo.WorkingSetMemory = process.WorkingSet64;
                            processInfo.ThreadCount = process.Threads.Count;
                            processInfo.HandleCount = process.HandleCount;
                            processInfo.PriorityClass = process.PriorityClass;
                            processInfo.SessionId = process.SessionId;
                            
                            // Try to get executable path
                            try
                            {
                                processInfo.ExecutablePath = process.MainModule?.FileName ?? string.Empty;
                            }
                            catch
                            {
                                // Some processes don't allow access to MainModule
                                processInfo.ExecutablePath = GetFullPathFromProcessName(processName) ?? string.Empty;
                            }
                            
                            // Try to get username
                            try
                            {
                                processInfo.UserName = GetProcessOwner(process.Id);
                            }
                            catch
                            {
                                // Some processes don't allow access to owner information
                            }
                        }
                        catch
                        {
                            // Skip additional details if we can't access them
                        }
                        
                        // Add to process dictionary
                        _processesById[process.Id] = processInfo;
                        
                        // Add to processes list
                        _processes.Add(processInfo);
                        
                        // If it has a window title, add to applications list and track window
                        if (!string.IsNullOrEmpty(windowTitle))
                        {
                            _applications.Add(processInfo);
                            
                            // Add window to windows list
                            _windows.Add(new WindowInfo
                            {
                                Title = windowTitle,
                                ProcessId = process.Id,
                                ProcessName = processName,
                                Is64Bit = is64Bit,
                                Handle = IntPtr.Zero // Will be set later if available
                            });
                            
                            // Track window by process ID
                            if (!windowsByProcessId.ContainsKey(process.Id))
                            {
                                windowsByProcessId[process.Id] = new List<string>();
                            }
                            windowsByProcessId[process.Id].Add(windowTitle);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Skip processes that can't be accessed
                        Logger.Log($"Error processing process {process.Id}: {ex.Message}", LogLevel.Debug);
                    }
                }
                
                // Second pass: Build process tree structure
                BuildProcessTree();
                
                // Get additional windows using Windows API
                GetAdditionalWindows(windowsByProcessId);
                
                // Sort lists
                SortLists();
                
                // Raise event
                OnProcessListRefreshed();
                
                Logger.Log($"Process list refreshed, found {_processes.Count} processes, {_applications.Count} applications, {_windows.Count} windows", LogLevel.Error);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error refreshing process list: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Builds the process tree structure
        /// </summary>
        private void BuildProcessTree()
        {
            try
            {
                // Clear root processes list
                _rootProcesses.Clear();
                
                // First pass: Set parent references
                foreach (var processInfo in _processesById.Values)
                {
                    // Try to find parent process
                    if (processInfo.ParentProcessId != 0 && _processesById.TryGetValue(processInfo.ParentProcessId, out ProcessInfo? parentProcess))
                    {
                        // Set parent reference
                        processInfo.Parent = parentProcess;
                        
                        // Add to parent's children
                        parentProcess.Children.Add(processInfo);
                    }
                    else
                    {
                        // No parent or parent not found, add to root list
                        _rootProcesses.Add(processInfo);
                    }
                }
                
                // Log tree structure
                Logger.Log($"Process tree built with {_rootProcesses.Count} root processes");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error building process tree: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Gets additional windows using Windows API
        /// </summary>
        private void GetAdditionalWindows(Dictionary<int, List<string>> existingWindowsByProcessId)
        {
            try
            {
                // Use Windows API to enumerate windows
                WinAPI.EnumWindows((hWnd, lParam) =>
                {
                    try
                    {
                        // Check if window is visible
                        if (!WinAPI.IsWindowVisible(hWnd))
                        {
                            return true;
                        }
                        
                        // Get window title
                        string title = WinAPI.GetWindowText(hWnd);
                        if (string.IsNullOrEmpty(title))
                        {
                            return true;
                        }
                        
                        // Apply filter if needed
                        if (!string.IsNullOrEmpty(_filterText) && !title.Contains(_filterText, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                        
                        // Get process ID
                        WinAPI.GetWindowThreadProcessId(hWnd, out uint processId);
                        int pid = (int)processId;
                        
                        // Skip if we already have this window
                        if (existingWindowsByProcessId.ContainsKey(pid) && existingWindowsByProcessId[pid].Contains(title))
                        {
                            return true;
                        }
                        
                        // Get process name
                        string processName = string.Empty;
                        try
                        {
                            using (Process process = Process.GetProcessById(pid))
                            {
                                processName = process.ProcessName;
                            }
                        }
                        catch
                        {
                            // Skip if we can't get process name
                            return true;
                        }
                        
                        // Add window to list
                        _windows.Add(new WindowInfo
                        {
                            Title = title,
                            ProcessId = pid,
                            ProcessName = processName,
                            Handle = hWnd
                        });
                    }
                    catch
                    {
                        // Skip windows that can't be accessed
                    }
                    
                    return true;
                }, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error enumerating windows: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Sorts the process and window lists
        /// </summary>
        private void SortLists()
        {
            // Sort processes by name
            List<ProcessInfo> sortedProcesses = _processes.OrderBy(p => p.Name).ToList();
            _processes.Clear();
            foreach (var process in sortedProcesses)
            {
                _processes.Add(process);
            }
            
            // Sort applications by name
            List<ProcessInfo> sortedApplications = _applications.OrderBy(p => p.Name).ToList();
            _applications.Clear();
            foreach (var app in sortedApplications)
            {
                _applications.Add(app);
            }
            
            // Sort windows by title
            List<WindowInfo> sortedWindows = _windows.OrderBy(w => w.Title).ToList();
            _windows.Clear();
            foreach (var window in sortedWindows)
            {
                _windows.Add(window);
            }
        }
        
        /// <summary>
        /// Applies a filter to the process lists
        /// </summary>
        /// <param name="filterText">The text to filter by</param>
        public void ApplyFilter(string filterText)
        {
            _filterText = filterText;
            RefreshProcessList();
        }
        
        /// <summary>
        /// Refreshes the process list asynchronously
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task RefreshProcessListAsync()
        {
            try
            {
                // Cancel any ongoing refresh operation
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = _cancellationTokenSource.Token;
                
                await Task.Run(() => 
                {
                    RefreshProcessList();
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Logger.Log("Process list refresh was canceled", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in async process list refresh: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Gets the parent process ID for a process
        /// </summary>
        /// <param name="processId">The process ID</param>
        /// <returns>The parent process ID, or 0 if not found</returns>
        private int GetParentProcessId(int processId)
        {
            // Check cache first
            if (_parentProcessIdCache.TryGetValue(processId, out int parentId))
            {
                return parentId;
            }
            
            try
            {
                // Try to get parent process ID using WMI
                string query = $"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {processId}";
                using (var searcher = new ManagementObjectSearcher(query))
                using (var results = searcher.Get())
                {
                    foreach (var result in results)
                    {
                        parentId = Convert.ToInt32(result["ParentProcessId"]);
                        
                        // Cache the result
                        _parentProcessIdCache[processId] = parentId;
                        
                        return parentId;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error getting parent process ID for {processId}: {ex.Message}", LogLevel.Debug);
            }
            
            // Cache the failure (0 means no parent or error)
            _parentProcessIdCache[processId] = 0;
            return 0;
        }
        
        // Duplicate GetProcessOwner method removed
        
        /// <summary>
        /// Gets the parent process ID for a process asynchronously
        /// </summary>
        /// <param name="processId">The process ID</param>
        /// <returns>A task representing the asynchronous operation that returns the parent process ID</returns>
        private async Task<int> GetParentProcessIdAsync(int processId)
        {
            // Check cache first
            if (_parentProcessIdCache.TryGetValue(processId, out int parentId))
            {
                return parentId;
            }
            
            try
            {
                // Try to get parent process ID using WMI asynchronously
                return await Task.Run(() =>
                {
                    string query = $"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {processId}";
                    using (var searcher = new ManagementObjectSearcher(query))
                    using (var results = searcher.Get())
                    {
                        foreach (var result in results)
                        {
                            parentId = Convert.ToInt32(result["ParentProcessId"]);
                            
                            // Cache the result
                            _parentProcessIdCache[processId] = parentId;
                            
                            return parentId;
                        }
                    }
                    
                    // Cache the failure (0 means no parent or error)
                    _parentProcessIdCache[processId] = 0;
                    return 0;
                });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error getting parent process ID for {processId}: {ex.Message}", LogLevel.Debug);
                
                // Cache the failure (0 means no parent or error)
                _parentProcessIdCache[processId] = 0;
                return 0;
            }
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
        /// Initializes a new instance of the <see cref="ProcessInfo"/> class
        /// </summary>
        public ProcessInfo()
        {
            Children = new List<ProcessInfo>();
            CachedProperties = new Dictionary<string, object>();
        }

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
        /// Gets or sets the parent process ID
        /// </summary>
        public int ParentProcessId { get; set; }

        /// <summary>
        /// Gets or sets the parent process
        /// </summary>
        public ProcessInfo? Parent { get; set; }

        /// <summary>
        /// Gets the list of child processes
        /// </summary>
        public List<ProcessInfo> Children { get; }

        /// <summary>
        /// Gets or sets the process creation time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the CPU usage percentage
        /// </summary>
        public float CpuUsage { get; set; }

        /// <summary>
        /// Gets or sets the private memory usage in bytes
        /// </summary>
        public long PrivateMemorySize { get; set; }

        /// <summary>
        /// Gets or sets the working set memory in bytes
        /// </summary>
        public long WorkingSetMemory { get; set; }

        /// <summary>
        /// Gets or sets the full path to the process executable
        /// </summary>
        public string ExecutablePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the command line
        /// </summary>
        public string CommandLine { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user name
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the session ID
        /// </summary>
        public int SessionId { get; set; }

        /// <summary>
        /// Gets or sets the number of threads
        /// </summary>
        public int ThreadCount { get; set; }

        /// <summary>
        /// Gets or sets the number of handles
        /// </summary>
        public int HandleCount { get; set; }

        /// <summary>
        /// Gets or sets whether the process is expanded in tree view
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// Gets or sets whether the process has been fully loaded with details
        /// </summary>
        public bool IsFullyLoaded { get; set; }

        /// <summary>
        /// Gets or sets the process priority class
        /// </summary>
        public ProcessPriorityClass PriorityClass { get; set; }

        /// <summary>
        /// Gets or sets the process icon key for image list
        /// </summary>
        public string IconKey { get; set; } = "default";

        /// <summary>
        /// Gets or sets a dictionary of cached properties to avoid repeated calculations
        /// </summary>
        public Dictionary<string, object> CachedProperties { get; }

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

        /// <summary>
        /// Gets the formatted private memory size
        /// </summary>
        public string FormattedPrivateMemory => FormatBytes(PrivateMemorySize);

        /// <summary>
        /// Gets the formatted working set memory
        /// </summary>
        public string FormattedWorkingSet => FormatBytes(WorkingSetMemory);

        /// <summary>
        /// Formats bytes into a human-readable string
        /// </summary>
        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n2} {suffixes[counter]}";
        }
    }
    
    /// <summary>
    /// Represents information about a window
    /// </summary>
    public class WindowInfo
    {
        /// <summary>
        /// Gets or sets the window title
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the process ID
        /// </summary>
        public int ProcessId { get; set; }
        
        /// <summary>
        /// Gets or sets the process name
        /// </summary>
        public string ProcessName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets whether the process is 64-bit
        /// </summary>
        public bool Is64Bit { get; set; }
        
        /// <summary>
        /// Gets or sets the window handle
        /// </summary>
        public IntPtr Handle { get; set; } = IntPtr.Zero;
    }
}
