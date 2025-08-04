using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CheatEngine.NET.Core;
using CheatEngine.NET.Utils;

namespace CheatEngine.NET.GUI
{
    /// <summary>
    /// Form for selecting a process to attach to, with filtering capabilities
    /// </summary>
    public partial class ProcessSelectForm : Form
    {
        private int _selectedProcessId;
        private string _selectedProcessName = string.Empty;
        private string _selectedProcessPath = string.Empty;
        private List<ProcessInfo> _allProcesses = new List<ProcessInfo>();
        private List<ProcessInfo> _filteredProcesses = new List<ProcessInfo>();
        private string _filterText = string.Empty;
        private ProcessCategory _currentCategory = ProcessCategory.Apps;
        
        /// <summary>
        /// Gets the selected process ID
        /// </summary>
        public int SelectedProcessId => _selectedProcessId;
        
        /// <summary>
        /// Gets the selected process name
        /// </summary>
        public string SelectedProcessName => _selectedProcessName;
        
        /// <summary>
        /// Gets the selected process path
        /// </summary>
        public string SelectedProcessPath => _selectedProcessPath;
        
        /// <summary>
        /// Enum for process categories
        /// </summary>
        private enum ProcessCategory
        {
            Apps = 0,
            Background = 1,
            Windows = 2
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessSelectForm"/> class
        /// </summary>
        public ProcessSelectForm()
        {
            InitializeComponent();
            
            // Set up event handlers
            this.Load += ProcessSelectForm_Load;
            
            // Set initial state
            _selectedProcessId = 0;
            _selectedProcessName = string.Empty;
            _selectedProcessPath = string.Empty;
        }
        
        /// <summary>
        /// Process information class
        /// </summary>
        private class ProcessInfo
        {
            /// <summary>
            /// Gets the process ID
            /// </summary>
            public int ProcessId { get; }
            
            /// <summary>
            /// Gets the process name
            /// </summary>
            public string Name { get; }
            
            /// <summary>
            /// Gets the process path
            /// </summary>
            public string Path { get; }
            
            /// <summary>
            /// Gets the process architecture
            /// </summary>
            public string Architecture { get; }
            
            /// <summary>
            /// Gets the process icon key
            /// </summary>
            public string IconKey { get; }
            
            /// <summary>
            /// Gets whether the process is a system process
            /// </summary>
            public bool IsSystemProcess { get; }
            
            /// <summary>
            /// Gets whether the process has a window
            /// </summary>
            public bool HasWindow { get; }
            
            /// <summary>
            /// Initializes a new instance of the <see cref="ProcessInfo"/> class
            /// </summary>
            public ProcessInfo(Process process, string iconKey, bool isSystemProcess, bool hasWindow)
            {
                ProcessId = process.Id;
                Name = process.ProcessName;
                Path = GetProcessPath(process);
                Architecture = Is64BitProcess(process) ? "x64" : "x86";
                IconKey = iconKey;
                IsSystemProcess = isSystemProcess;
                HasWindow = hasWindow;
            }
            
            /// <summary>
            /// Gets the full path of a process
            /// </summary>
            private string GetProcessPath(Process process)
            {
                try
                {
                    return process.MainModule?.FileName ?? string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            }
            
            /// <summary>
            /// Checks if a process is 64-bit
            /// </summary>
            private bool Is64BitProcess(Process process)
            {
                if (!Environment.Is64BitOperatingSystem)
                    return false;
                    
                try
                {
                    bool isWow64;
                    if (!IsWow64Process(process.Handle, out isWow64))
                        return false;
                        
                    return !isWow64;
                }
                catch
                {
                    return false;
                }
            }
        }
        
        /// <summary>
        /// Initializes the image list with default icons
        /// </summary>
        private void InitializeImageList()
        {
            try
            {
                // Configure image list
                imageList.ColorDepth = ColorDepth.Depth32Bit;
                imageList.ImageSize = new Size(16, 16);
                imageList.TransparentColor = Color.Transparent;
                
                // Add default icons
                imageList.Images.Add("default", SystemIcons.Application);
                imageList.Images.Add("system", SystemIcons.Shield);
                
                // Pre-load some common application icons for better performance
                PreloadCommonIcons();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error initializing image list: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Preloads icons for common applications
        /// </summary>
        private void PreloadCommonIcons()
        {
            try
            {
                // Add icon for this application
                string exePath = Process.GetCurrentProcess().MainModule.FileName;
                AddIconFromFile(exePath, Path.GetFileNameWithoutExtension(exePath));
                
                // Add icons for common Windows applications
                string system32Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "");
                string explorerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
                
                AddIconFromFile(explorerPath, "explorer");
                AddIconFromFile(Path.Combine(system32Path, "cmd.exe"), "cmd");
                AddIconFromFile(Path.Combine(system32Path, "notepad.exe"), "notepad");
                AddIconFromFile(Path.Combine(system32Path, "taskmgr.exe"), "taskmgr");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error preloading common icons: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Gets the full path of a process
        /// </summary>
        private string GetProcessPath(Process process)
        {
            try
            {
                return process.MainModule?.FileName ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Checks if a process is 64-bit
        /// </summary>
        private bool Is64BitProcess(Process process)
        {
            if (!Environment.Is64BitOperatingSystem)
                return false;
                
            try
            {
                bool isWow64;
                if (!IsWow64Process(process.Handle, out isWow64))
                    return false;
                    
                return !isWow64;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Checks if a process is a system process
        /// </summary>
        private bool IsSystemProcess(Process process)
        {
            try
            {
                string[] systemProcessNames = new[]
                {
                    "svchost", "csrss", "smss", "wininit", "winlogon", "services", "lsass", "spoolsv", "dwm",
                    "explorer", "taskmgr", "taskhost", "conhost", "RuntimeBroker", "ShellExperienceHost"
                };
                
                return systemProcessNames.Contains(process.ProcessName.ToLower());
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Checks if a process has a visible window
        /// </summary>
        private bool HasVisibleWindow(Process process)
        {
            try
            {
                return !string.IsNullOrEmpty(process.MainWindowTitle) && process.MainWindowHandle != IntPtr.Zero;
            }
            catch
            {
                return false;
            }
        }
        
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);
        
        /// <summary>
        /// Adds an icon from a file to the image list
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <param name="key">The key for the icon</param>
        private void AddIconFromFile(string filePath, string key)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    Icon icon = Icon.ExtractAssociatedIcon(filePath);
                    if (icon != null)
                    {
                        imageList.Images.Add(key, icon);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error adding icon from {filePath}: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Gets the icon key for a process
        /// </summary>
        private string GetProcessIconKey(Process process)
        {
            try
            {
                string processName = process.ProcessName.ToLower();
                string iconKey = processName;
                
                // Check if we already have this icon
                if (imageList.Images.ContainsKey(iconKey))
                    return iconKey;
                
                // Try to get the icon from the process path
                string processPath = GetProcessPath(process);
                if (!string.IsNullOrEmpty(processPath) && File.Exists(processPath))
                {
                    try
                    {
                        Icon icon = Icon.ExtractAssociatedIcon(processPath);
                        if (icon != null)
                        {
                            imageList.Images.Add(iconKey, icon);
                            return iconKey;
                        }
                    }
                    catch
                    {
                        // Fall back to default icon
                    }
                }
                
                // Use system icon for system processes
                if (IsSystemProcess(process))
                    return "system";
                    
                return "default";
            }
            catch
            {
                return "default";
            }
        }
        
        /// <summary>
        /// Handles the Load event of the ProcessSelectForm
        /// </summary>
        private void ProcessSelectForm_Load(object sender, EventArgs e)
        {
            // Configure ListView
            processListView.FullRowSelect = true;
            processListView.MultiSelect = false;
            processListView.View = View.Details;
            processListView.SmallImageList = imageList;
            
            // Enable grouping
            processListView.ShowGroups = true;
            processListView.Groups.Clear();
            processListView.Groups.Add(new ListViewGroup("Process Apps", HorizontalAlignment.Left));
            processListView.Groups.Add(new ListViewGroup("Background Processes", HorizontalAlignment.Left));
            processListView.Groups.Add(new ListViewGroup("Windows Processes", HorizontalAlignment.Left));
            
            // Increase row height for better visibility
            ImageList rowHeightList = new ImageList();
            rowHeightList.ImageSize = new Size(1, 28); // Set row height to 28 pixels
            processListView.StateImageList = rowHeightList;
            
            // Set up event handlers
            processListView.SelectedIndexChanged += ProcessListView_SelectedIndexChanged;
            processListView.MouseDoubleClick += ProcessListView_MouseDoubleClick;
            txtFilter.TextChanged += TxtFilter_TextChanged;
            btnRefresh.Click += BtnRefresh_Click;
            
            // Initialize image list
            InitializeImageList();
            
            // Fill the process list when the form loads
            FillProcessList();
        }
        
        /// <summary>
        /// Fills the process list with running processes
        /// </summary>
        private void FillProcessList()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                processListView.BeginUpdate();
                processListView.Items.Clear();
                _allProcesses.Clear();
                
                // Get all processes
                Process[] processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    try
                    {
                        // Get process icon
                        string iconKey = GetProcessIconKey(process);
                        
                        // Determine if this is a system process
                        bool isSystemProcess = IsSystemProcess(process);
                        
                        // Determine if this process has a window
                        bool hasWindow = HasVisibleWindow(process);
                        
                        // Create process info
                        ProcessInfo processInfo = new ProcessInfo(process, iconKey, isSystemProcess, hasWindow);
                        _allProcesses.Add(processInfo);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error processing process {process.ProcessName}: {ex.Message}", LogLevel.Error);
                    }
                }
                
                // Apply filtering based on current tab
                ApplyFiltering();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error filling process list: {ex.Message}", LogLevel.Error);
                MessageBox.Show($"Error filling process list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                processListView.EndUpdate();
                Cursor = Cursors.Default;
            }
        }
        
        /// <summary>
        /// Applies filtering to the process list based on the filter text and groups processes by category
        /// </summary>
        private void ApplyFiltering()
        {
            try
            {
                processListView.BeginUpdate();
                processListView.Items.Clear();
                _filteredProcesses.Clear();
                
                // Filter processes based on text filter
                foreach (ProcessInfo process in _allProcesses)
                {
                    bool include = true;
                    
                    // Apply text filter if specified
                    if (!string.IsNullOrEmpty(_filterText))
                    {
                        include = process.Name.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 process.ProcessId.ToString().IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 process.Architecture.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                    
                    if (include)
                    {
                        _filteredProcesses.Add(process);
                    }
                }
                
                // Add filtered processes to the list view with appropriate groups
                foreach (ProcessInfo process in _filteredProcesses)
                {
                    ListViewItem item = new ListViewItem(process.ProcessId.ToString());
                    item.SubItems.Add(process.Architecture);
                    item.SubItems.Add(process.Name);
                    item.ImageKey = process.IconKey;
                    item.Tag = process;
                    
                    // Assign to appropriate group
                    if (process.HasWindow && !process.IsSystemProcess)
                    {
                        // Process Apps group
                        item.Group = processListView.Groups[0];
                    }
                    else if (!process.HasWindow && !process.IsSystemProcess)
                    {
                        // Background Processes group
                        item.Group = processListView.Groups[1];
                    }
                    else if (process.IsSystemProcess)
                    {
                        // Windows Processes group
                        item.Group = processListView.Groups[2];
                    }
                    
                    processListView.Items.Add(item);
                }
                
                // Update status label
                lblSelectProcess.Text = $"Select Process: ({_filteredProcesses.Count} processes)";
            }
            finally
            {
                processListView.EndUpdate();
            }
        }
        
        /// <summary>
        /// Handles the SelectedIndexChanged event of the ProcessListView
        /// </summary>
        private void ProcessListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (processListView.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = processListView.SelectedItems[0];
                ProcessInfo process = (ProcessInfo)selectedItem.Tag;
                
                _selectedProcessId = process.ProcessId;
                _selectedProcessName = process.Name;
                _selectedProcessPath = process.Path;
                
                btnOK.Enabled = true;
            }
            else
            {
                _selectedProcessId = 0;
                _selectedProcessName = string.Empty;
                _selectedProcessPath = string.Empty;
                
                btnOK.Enabled = false;
            }
        }
        
        /// <summary>
        /// Handles the MouseDoubleClick event of the ProcessListView
        /// </summary>
        private void ProcessListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (processListView.SelectedItems.Count > 0)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        
        /// <summary>
        /// Handles the TextChanged event of the TxtFilter
        /// </summary>
        private void TxtFilter_TextChanged(object sender, EventArgs e)
        {
            _filterText = txtFilter.Text.Trim();
            ApplyFiltering();
        }
        
        /// <summary>
        /// Handles the Click event of the BtnRefresh
        /// </summary>
        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            FillProcessList();
        }
        
        /// <summary>
        /// Handles the Click event of the BtnOK
        /// </summary>
        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (processListView.SelectedItems.Count > 0)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        
        /// <summary>
        /// Handles the Click event of the BtnCancel
        /// </summary>
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
