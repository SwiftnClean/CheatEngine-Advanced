using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CheatEngine.NET.Core;
using CheatEngine.NET.Utils;

namespace CheatEngine.NET.GUI
{
    /// <summary>
    /// Form for selecting a process to attach to
    /// </summary>
    public partial class ProcessListForm : Form
    {
        private readonly ProcessListManager _processListManager;
        private bool _isRefreshing = false;
        private int _selectedProcessId;
        private string _selectedProcessName = string.Empty;
        
        /// <summary>
        /// Gets the selected process ID
        /// </summary>
        public int SelectedProcessId { get; private set; }
        
        /// <summary>
        /// Gets the selected process name
        /// </summary>
        public string SelectedProcessName { get; private set; } = string.Empty;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessListForm"/> class
        /// </summary>
        public ProcessListForm()
        {
            InitializeComponent();
            
            // Create process list manager
            _processListManager = new ProcessListManager();
            
            // Set up data binding
            processListView.VirtualMode = true;
            processListView.RetrieveVirtualItem += ProcessListView_RetrieveVirtualItem;
            
            applicationsListView.VirtualMode = true;
            applicationsListView.RetrieveVirtualItem += ApplicationsListView_RetrieveVirtualItem;
            
            windowsListView.VirtualMode = true;
            windowsListView.RetrieveVirtualItem += WindowsListView_RetrieveVirtualItem;
            
            // Set up event handlers
            _processListManager.ProcessListRefreshed += ProcessListManager_ProcessListRefreshed;
            
            // Set up filter checkboxes
            showSystemProcessesCheckBox.Checked = _processListManager.ShowSystemProcesses;
            show64BitProcessesCheckBox.Checked = _processListManager.Show64BitProcesses;
            show32BitProcessesCheckBox.Checked = _processListManager.Show32BitProcesses;
            
            // Initialize image list with default icons
            InitializeImageList();
            
            // Initial refresh
            RefreshProcessList();
        }
        
        /// <summary>
        /// Initializes the image list with default icons
        /// </summary>
        private void InitializeImageList()
        {
            try
            {
                // Clear and configure image list
                imageList.Images.Clear();
                imageList.ColorDepth = ColorDepth.Depth32Bit;
                imageList.ImageSize = new Size(16, 16);
                imageList.TransparentColor = Color.Transparent;
                
                // Add default icons
                imageList.Images.Add("default", SystemIcons.Application);
                imageList.Images.Add("process", SystemIcons.Application);
                
                // Set the image list for all list views
                processListView.SmallImageList = imageList;
                applicationsListView.SmallImageList = imageList;
                windowsListView.SmallImageList = imageList;
                
                // Pre-load some common application icons for better performance
                PreloadCommonIcons();
            }
            catch (Exception ex)
            {
                // Log the error but continue
                Logger.Log($"Error initializing image list: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Handles the form load event
        /// </summary>
        private void ProcessListForm_Load(object sender, EventArgs e)
        {
            // Add event handlers for list view item clicks to handle tree expansion/collapse
            applicationsListView.MouseClick += ListView_MouseClick;
            processListView.MouseClick += ListView_MouseClick;
            windowsListView.MouseClick += ListView_MouseClick;
            
            // Set up columns - New layout with PID, Process Name, Architecture
            processListView.Columns.Clear();
            processListView.Columns.Add("", 20);
            processListView.Columns.Add("PID", 80);
            processListView.Columns.Add("Process", 350);
            processListView.Columns.Add("Architecture", 100);
            
            applicationsListView.Columns.Clear();
            applicationsListView.Columns.Add("", 20);
            applicationsListView.Columns.Add("PID", 80);
            applicationsListView.Columns.Add("Process", 350);
            applicationsListView.Columns.Add("Architecture", 100);
            
            windowsListView.Columns.Clear();
            windowsListView.Columns.Add("", 20);
            windowsListView.Columns.Add("PID", 80);
            windowsListView.Columns.Add("Process", 350);
            windowsListView.Columns.Add("Architecture", 100);
            
            // Initialize image list with icons
            InitializeImageList();
            
            // Hide the system processes checkbox since we always show system processes now
            showSystemProcessesCheckBox.Visible = false;
            
            // Initial refresh
            RefreshProcessList();
        }

        /// <summary>
        /// Refreshes the process list
        /// </summary>
        private async void RefreshProcessList()
        {
            if (_isRefreshing)
                return;
            
            _isRefreshing = true;
            btnRefresh.Enabled = false;
            statusLabel.Text = "Refreshing process list...";
            
            try
            {
                // Clear existing items
                processListView.Items.Clear();
                applicationsListView.Items.Clear();
                windowsListView.Items.Clear();
                
                // Refresh process list asynchronously
                await _processListManager.RefreshProcessListAsync();
                
                // Update status
                statusLabel.Text = "Building process tree...";
                Application.DoEvents(); // Allow UI to update
                
                // Populate the list views with hierarchical process tree
                await Task.Run(() => 
                {
                    // Use BeginInvoke to update UI from background thread
                    this.BeginInvoke(new Action(() => 
                    {
                        try
                        {
                            // Log process counts
                            Logger.Log($"Before populating: Applications: {_processListManager.Applications.Count}, Processes: {_processListManager.Processes.Count}, Windows: {_processListManager.Windows.Count}");
                            
                            // Populate each tab with hierarchical process tree
                            PopulateProcessListView(applicationsListView, _processListManager.Applications);
                            PopulateProcessListView(processListView, _processListManager.Processes);
                            PopulateWindowsListView(windowsListView, _processListManager.Windows);
                            
                            // Update tab texts with process counts
                            applicationsTabPage.Text = $"Process Apps ({_processListManager.Applications.Count})";
                            processesTabPage.Text = $"Background Processes ({_processListManager.Processes.Count})";
                            windowsTabPage.Text = $"Windows Processes ({_processListManager.Windows.Count})";
                            
                            Logger.Log("Process tree views populated successfully", LogLevel.Error);
                            Logger.Log($"ListView items: Applications: {applicationsListView.Items.Count}, Processes: {processListView.Items.Count}, Windows: {windowsListView.Items.Count}", LogLevel.Error);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"Error populating process trees: {ex.Message}", LogLevel.Error);
                        }
                    }));
                });
                
                // Update status
                statusLabel.Text = $"Ready. Found {_processListManager.Processes.Count} processes.";
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error: {ex.Message}";
                Logger.Log($"Error refreshing process list: {ex.Message}", LogLevel.Error);
            }
            finally
            {
                btnRefresh.Enabled = true;
                _isRefreshing = false;
            }
        }
        
        /// <summary>
        /// Handles the process list refreshed event
        /// </summary>
        private void ProcessListManager_ProcessListRefreshed(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(ProcessListManager_ProcessListRefreshed), sender, e);
                return;
            }
            
            // Update status
            statusLabel.Text = $"Found {_processListManager.Processes.Count} processes";
        }
        
        /// <summary>
        /// Handles the retrieve virtual item event for the process list view
        /// </summary>
        private void ProcessListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            try
            {
                if (e.ItemIndex >= 0 && e.ItemIndex < _processListManager.Processes.Count)
                {
                    var process = _processListManager.Processes[e.ItemIndex];
                    
                    ListViewItem item = new ListViewItem();
                    
                    // Get process icon
                    string iconKey = GetProcessIconKey(process.Id, process.Name);
                    item.ImageKey = iconKey;
                    
                    // Add PID
                    item.SubItems.Add(process.Id.ToString());
                    
                    // Add process name
                    item.SubItems.Add(process.Name);
                    
                    // Add architecture information
                    string architecture = process.Is64Bit ? "64 bit" : "32 bit";
                    item.SubItems.Add(architecture);
                    
                    item.Tag = process;
                    
                    e.Item = item;
                }
                else
                {
                    // Create an empty item if the index is out of range
                    e.Item = new ListViewItem();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error retrieving virtual item: {ex.Message}", LogLevel.Error);
                e.Item = new ListViewItem();
            }
        }
        
        /// <summary>
        /// Handles the retrieve virtual item event for the applications list view
        /// </summary>
        private void ApplicationsListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            try
            {
                if (_processListManager.Applications != null &&
                    e.ItemIndex >= 0 && e.ItemIndex < _processListManager.Applications.Count)
                {
                    var process = _processListManager.Applications[e.ItemIndex];
                    
                    ListViewItem item = new ListViewItem();
                    
                    // Get process icon
                    string iconKey = GetProcessIconKey(process.Id, process.Name);
                    item.ImageKey = iconKey;
                    
                    // Add PID
                    item.SubItems.Add(process.Id.ToString());
                    
                    // Add process name
                    item.SubItems.Add(process.Name);
                    
                    // Add architecture information
                    string architecture = process.Is64Bit ? "64 bit" : "32 bit";
                    item.SubItems.Add(architecture);
                    
                    item.Tag = process;
                    
                    e.Item = item;
                }
                else
                {
                    // Create an empty item if the index is out of range
                    e.Item = new ListViewItem();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error retrieving virtual item: {ex.Message}", LogLevel.Error);
                e.Item = new ListViewItem();
            }
        }
        
        /// <summary>
        /// Handles the retrieve virtual item event for the windows list view
        /// </summary>
        private void WindowsListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            try
            {
                if (_processListManager.Windows != null &&
                    e.ItemIndex >= 0 && e.ItemIndex < _processListManager.Windows.Count)
                {
                    var window = _processListManager.Windows[e.ItemIndex];
                    
                    ListViewItem item = new ListViewItem();
                    
                    // Get process icon
                    string iconKey = GetProcessIconKey(window.ProcessId, window.ProcessName);
                    item.ImageKey = iconKey;
                    
                    // Add PID
                    item.SubItems.Add(window.ProcessId.ToString());
                    
                    // Add process name
                    item.SubItems.Add(window.ProcessName);
                    
                    // Add architecture information (N/A for windows)
                    item.SubItems.Add("N/A");
                    
                    item.Tag = window;
                    
                    e.Item = item;
                }
                else
                {
                    // Create an empty item if the index is out of range
                    e.Item = new ListViewItem();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error retrieving virtual item: {ex.Message}", LogLevel.Error);
                e.Item = new ListViewItem();
            }
        }
        
        /// <summary>
        /// Handles the show system processes checkbox checked changed event
        /// </summary>
        private void showSystemProcessesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // No-op, we always show system processes now
            // Checkbox is hidden in the UI
        }
        
        /// <summary>
        /// Handles the show 64-bit processes checkbox checked changed event
        /// </summary>
        private void show64BitProcessesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _processListManager.Show64BitProcesses = show64BitProcessesCheckBox.Checked;
            RefreshProcessList();
        }
        
        /// <summary>
        /// Handles the show 32-bit processes checkbox checked changed event
        /// </summary>
        private void show32BitProcessesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _processListManager.Show32BitProcesses = show32BitProcessesCheckBox.Checked;
            RefreshProcessList();
        }
        
        /// <summary>
        /// Handles the filter text box text changed event
        /// </summary>
        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            // Apply filter text
            _processListManager.ApplyFilter(txtFilter.Text);
            RefreshProcessList();
        }
        
        /// <summary>
        /// Handles the tab control selected index changed event
        /// </summary>
        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedProcess();
        }
        
        /// <summary>
        /// Updates the selected process based on the current tab
        /// </summary>
        private void UpdateSelectedProcess()
        {
            // Get the selected item based on the active tab
            ListViewItem? selectedItem = null;
            
            switch (tabControl.SelectedIndex)
            {
                case 0: // Process Apps
                    if (applicationsListView.SelectedIndices.Count > 0)
                        selectedItem = applicationsListView.Items[applicationsListView.SelectedIndices[0]];
                    break;
                    
                case 1: // Background Processes
                    if (processListView.SelectedIndices.Count > 0)
                        selectedItem = processListView.Items[processListView.SelectedIndices[0]];
                    break;
                    
                case 2: // Windows Processes
                    if (windowsListView.SelectedIndices.Count > 0)
                        selectedItem = windowsListView.Items[windowsListView.SelectedIndices[0]];
                    break;
            }
            
            // Update the selected process if an item is selected
            if (selectedItem != null && selectedItem.Tag != null)
            {
                if (selectedItem.Tag is ProcessInfo processInfo)
                {
                    _selectedProcessId = processInfo.Id;
                    _selectedProcessName = processInfo.Name;
                }
                else if (selectedItem.Tag is WindowInfo windowInfo)
                {
                    _selectedProcessId = windowInfo.ProcessId;
                    _selectedProcessName = windowInfo.ProcessName;
                }
            }
        }
        
        /// <summary>
        /// Preloads common application icons to improve performance
        /// </summary>
        private void PreloadCommonIcons()
        {
            try
            {
                // Add a default icon for processes without icons
                imageList.Images.Add("default", SystemIcons.Application);
                
                // Common system applications to preload icons for
                string[] commonApps = new string[]
                {
                    "explorer.exe",
                    "chrome.exe",
                    "firefox.exe",
                    "msedge.exe",
                    "devenv.exe",
                    "notepad.exe",
                    "cmd.exe",
                    "powershell.exe",
                    "svchost.exe",
                    "System"
                };
                
                // Add our own application's icon
                string ownExecutablePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(ownExecutablePath) && File.Exists(ownExecutablePath))
                {
                    try
                    {
                        string processName = Path.GetFileName(ownExecutablePath).ToLower();
                        Icon icon = Icon.ExtractAssociatedIcon(ownExecutablePath);
                        if (icon != null && !imageList.Images.ContainsKey(processName))
                        {
                            imageList.Images.Add(processName, icon);
                            Logger.Log($"Added icon for current process: {processName}", LogLevel.Info);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error adding own process icon: {ex.Message}", LogLevel.Warning);
                    }
                }
                
                foreach (string app in commonApps)
                {
                    try
                    {
                        string path = GetFullPathFromProcessName(app);
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        {
                            string processName = app.ToLower();
                            if (!imageList.Images.ContainsKey(processName))
                            {
                                Icon icon = Icon.ExtractAssociatedIcon(path);
                                if (icon != null)
                                {
                                    imageList.Images.Add(processName, icon);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error preloading icon for {app}: {ex.Message}", LogLevel.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in PreloadCommonIcons: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Updates the tree structure in each list view to enable hierarchical process exploration
        /// </summary>
        private async Task UpdateProcessTreeStructure()
        {
            try
            {
                // Get all processes including the current application
                var allProcesses = new List<ProcessInfo>(_processListManager.Processes);
                allProcesses.AddRange(_processListManager.Applications);
                // Add window processes - need to look up Is64Bit from process dictionary
                var processDict = _processListManager.Processes.ToDictionary(p => p.Id);
                foreach (var window in _processListManager.Windows)
                {
                    bool is64Bit = false;
                    if (processDict.TryGetValue(window.ProcessId, out var proc))
                    {
                        is64Bit = proc.Is64Bit;
                    }
                    
                    allProcesses.Add(new ProcessInfo { Id = window.ProcessId, Name = window.ProcessName, Is64Bit = is64Bit });
                }
                
                // Make sure our own process is included
                var currentProcess = Process.GetCurrentProcess();
                bool hasOwnProcess = allProcesses.Any(p => p.Id == currentProcess.Id);
                
                if (!hasOwnProcess)
                {
                    // Add our own process if it's not already in the list
                    var ownProcessInfo = new ProcessInfo
                    {
                        Id = currentProcess.Id,
                        Name = currentProcess.ProcessName,
                        Is64Bit = Environment.Is64BitProcess
                    };
                    
                    allProcesses.Add(ownProcessInfo);
                }
                
                // Create a dictionary of parent-child relationships
                Dictionary<int, List<int>> processChildren = new Dictionary<int, List<int>>();
                Dictionary<int, ProcessInfo> processById = new Dictionary<int, ProcessInfo>();
                
                // Show progress in status bar
                statusLabel.Text = "Building process tree structure...";
                Application.DoEvents(); // Allow UI to update
                
                // First pass: build dictionaries - do this in parallel to improve performance
                foreach (var process in allProcesses)
                {
                    if (!processById.ContainsKey(process.Id))
                    {
                        processById[process.Id] = process;
                    }
                }
                
                // Get all parent process IDs in parallel for better performance
                var parentProcessTasks = new Dictionary<int, Task<int>>();
                foreach (var process in allProcesses)
                {
                    parentProcessTasks[process.Id] = GetParentProcessIdAsync(process.Id);
                }
                
                // Wait for all parent process ID lookups to complete
                await Task.WhenAll(parentProcessTasks.Values);
                
                // Build the parent-child relationships
                foreach (var process in allProcesses)
                {
                    int parentId = await parentProcessTasks[process.Id];
                    
                    if (parentId > 0)
                    {
                        if (!processChildren.ContainsKey(parentId))
                        {
                            processChildren[parentId] = new List<int>();
                        }
                        
                        processChildren[parentId].Add(process.Id);
                    }
                }
                
                // Store the parent-child relationships in the tag property of each list item
                // This allows us to expand/collapse child processes when a parent is clicked
                
                // Update applications list view
                foreach (ListViewItem item in applicationsListView.Items)
                {
                    if (item.Tag is ProcessInfo process)
                    {
                        // Store child process IDs in the tag
                        if (processChildren.ContainsKey(process.Id))
                        {
                            item.Tag = new { Process = process, Children = processChildren[process.Id] };
                            // Add a + indicator to show it has children
                            item.Text = "+ " + item.Text;
                        }
                    }
                }
                
                // Update processes list view
                foreach (ListViewItem item in processListView.Items)
                {
                    if (item.Tag is ProcessInfo process)
                    {
                        // Store child process IDs in the tag
                        if (processChildren.ContainsKey(process.Id))
                        {
                            item.Tag = new { Process = process, Children = processChildren[process.Id] };
                            // Add a + indicator to show it has children
                            item.Text = "+ " + item.Text;
                        }
                    }
                }
                
                // Update windows list view
                foreach (ListViewItem item in windowsListView.Items)
                {
                    if (item.Tag is WindowInfo window)
                    {
                        // Store child process IDs in the tag
                        if (processChildren.ContainsKey(window.ProcessId))
                        {
                            item.Tag = new { Window = window, Children = processChildren[window.ProcessId] };
                            // Add a + indicator to show it has children
                            item.Text = "+ " + item.Text;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error updating process tree structure: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Creates a TreeNode for a process
        /// </summary>
        private TreeNode CreateProcessNode(ProcessInfo process)
        {
            string iconKey = GetProcessIconKey(process.Id, process.Name);
            string nodeText = $"{process.Name} ({process.Id})";
            string architecture = process.Is64Bit ? "64-bit" : "32-bit";
            
            TreeNode node = new TreeNode(nodeText);
            node.ImageKey = iconKey;
            node.SelectedImageKey = iconKey;
            node.Tag = process;
            node.ToolTipText = $"PID: {process.Id}\r\nName: {process.Name}\r\nArchitecture: {architecture}";
            
            return node;
        }
        
        /// <summary>
        /// Recursively adds child process nodes to a parent node
        /// </summary>
        private void AddChildProcessNodes(TreeNode parentNode, int parentProcessId, Dictionary<int, List<ProcessInfo>> processChildren)
        {
            if (!processChildren.ContainsKey(parentProcessId))
                return;
                
            var children = processChildren[parentProcessId];
            children.Sort((a, b) => a.Name.CompareTo(b.Name)); // Sort by name
            
            foreach (var childProcess in children)
            {
                var childNode = CreateProcessNode(childProcess);
                parentNode.Nodes.Add(childNode);
                
                // Recursively add children
                if (processChildren.ContainsKey(childProcess.Id))
                {
                    AddChildProcessNodes(childNode, childProcess.Id, processChildren);
                }
            }
        }
        
        /// <summary>
        /// Handles mouse clicks on list view items to expand/collapse the tree structure
        /// </summary>
        private void ListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (sender is ListView listView)
            {
                // Get the clicked item
                ListViewItem clickedItem = listView.GetItemAt(e.X, e.Y);
                
                if (clickedItem != null)
                {
                    // Check if the item has children (indicated by the + prefix)
                    if (clickedItem.Text.StartsWith("+ "))
                    {
                        // This is a collapsed node with children - expand it
                        clickedItem.Text = clickedItem.Text.Replace("+ ", "- ");
                        ExpandProcessNode(listView, clickedItem);
                    }
                    else if (clickedItem.Text.StartsWith("- "))
                    {
                        // This is an expanded node - collapse it
                        clickedItem.Text = clickedItem.Text.Replace("- ", "+ ");
                        CollapseProcessNode(listView, clickedItem);
                    }
                }
            }
        }
        
        /// <summary>
        /// Expands a process node to show its child processes
        /// </summary>
        private void ExpandProcessNode(ListView listView, ListViewItem parentItem)
        {
            try
            {
                // Get the child process IDs from the tag
                List<int> childProcessIds = null;
                int parentProcessId = 0;
                
                // Extract child process IDs based on the item type
                if (parentItem.Tag != null)
                {
                    try
                    {
                        // Check if the tag contains a ProcessInfo object with children
                        if (parentItem.Tag is object tagObj)
                        {
                            Type tagType = tagObj.GetType();
                            
                            // Try to access the Children property using reflection
                            var childrenProp = tagType.GetProperty("Children");
                            if (childrenProp != null)
                            {
                                childProcessIds = childrenProp.GetValue(tagObj) as List<int>;
                            }
                            
                            // Try to get the parent process ID
                            var processProp = tagType.GetProperty("Process");
                            if (processProp != null)
                            {
                                var processObj = processProp.GetValue(tagObj);
                                if (processObj is ProcessInfo pi)
                                {
                                    parentProcessId = pi.Id;
                                }
                            }
                            else
                            {
                                var windowProp = tagType.GetProperty("Window");
                                if (windowProp != null)
                                {
                                    var windowObj = windowProp.GetValue(tagObj);
                                    if (windowObj is WindowInfo wi)
                                    {
                                        parentProcessId = wi.ProcessId;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error accessing tag properties: {ex.Message}", LogLevel.Error);
                        return;
                    }
                }
                
                if (childProcessIds == null || childProcessIds.Count == 0)
                {
                    return;
                }
                
                // Get all processes
                var allProcesses = new List<ProcessInfo>();
                allProcesses.AddRange(_processListManager.Processes);
                allProcesses.AddRange(_processListManager.Applications);
                
                // Create a dictionary for quick lookup
                Dictionary<int, ProcessInfo> processById = allProcesses.ToDictionary(p => p.Id);
                
                // Find the index where we need to insert child items
                int insertIndex = parentItem.Index + 1;
                
                // Add child items
                foreach (int childId in childProcessIds)
                {
                    if (processById.TryGetValue(childId, out ProcessInfo childProcess))
                    {
                        // Create a new item for the child process
                        ListViewItem childItem = new ListViewItem();
                        childItem.Text = "    " + childProcess.Name; // Indent to show hierarchy
                        childItem.SubItems.Add(childProcess.Id.ToString());
                        childItem.SubItems.Add(childProcess.Name);
                        childItem.SubItems.Add(childProcess.Is64Bit ? "64-bit" : "32-bit");
                        childItem.Tag = childProcess;
                        
                        // Set icon
                        string iconKey = GetProcessIconKey(childProcess.Id, childProcess.Name);
                        if (imageList.Images.ContainsKey(iconKey))
                        {
                            childItem.ImageKey = iconKey;
                        }
                        
                        // Insert the child item after the parent
                        listView.Items.Insert(insertIndex++, childItem);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error expanding process node: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Collapses a process node to hide its child processes
        /// </summary>
        private void CollapseProcessNode(ListView listView, ListViewItem parentItem)
        {
            try
            {
                // Find all child items (they are indented with spaces)
                int index = parentItem.Index + 1;
                
                while (index < listView.Items.Count)
                {
                    ListViewItem item = listView.Items[index];
                    
                    // Check if this is a child item (indented)
                    if (item.Text.StartsWith("    "))
                    {
                        // Remove this child item
                        listView.Items.RemoveAt(index);
                        // Don't increment index since we removed an item
                    }
                    else
                    {
                        // We've reached the end of the children
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error collapsing process node: {ex.Message}", LogLevel.Error);
            }
        }
        
        // Cache for parent process IDs to avoid repeated WMI queries
        private Dictionary<int, int> _parentProcessCache = new Dictionary<int, int>();
        
        /// <summary>
        /// Gets the parent process ID for a given process ID
        /// </summary>
        private async Task<int> GetParentProcessIdAsync(int processId)
        {
            try
            {
                // Check if the process exists in the ProcessListManager's dictionary
                var processes = _processListManager.Processes;
                var processDict = processes.ToDictionary(p => p.Id);
                
                if (processDict.TryGetValue(processId, out var processInfo))
                {
                    // If ParentProcessId is already set, return it
                    if (processInfo.ParentProcessId > 0)
                    {
                        return processInfo.ParentProcessId;
                    }
                }
                
                // Otherwise, use WMI to get the parent process ID
                return await Task.Run(() =>
                {
                    try
                    {
                        string query = $"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {processId}";
                        using (var searcher = new ManagementObjectSearcher(query))
                        using (var results = searcher.Get())
                        {
                            foreach (var result in results)
                            {
                                int parentId = Convert.ToInt32(result["ParentProcessId"]);
                                
                                // If we found the process in our dictionary, update its ParentProcessId
                                if (processDict.TryGetValue(processId, out var process))
                                {
                                    process.ParentProcessId = parentId;
                                }
                                
                                return parentId;
                            }
                        }
                        
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error getting parent process ID: {ex.Message}", LogLevel.Debug);
                        return 0;
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in GetParentProcessIdAsync: {ex.Message}", LogLevel.Error);
                return 0;
            }
        }
        
        /// <summary>
        /// Handles the process list view double click event
        /// </summary>
        private void processListView_DoubleClick(object sender, EventArgs e)
        {
            if (processListView.SelectedItems.Count > 0)
            {
                var item = processListView.SelectedItems[0];
                if (item.Tag is ProcessInfo process)
                {
                    SelectedProcessId = process.Id;
                    SelectedProcessName = process.Name;
                    DialogResult = DialogResult.OK;
                }
                DialogResult = DialogResult.OK;
            }
        }
        
        /// <summary>
        /// Gets the full path of an executable from its process name
        /// </summary>
        private string GetFullPathFromProcessName(string processName)
        {
            if (string.IsNullOrEmpty(processName))
                return string.Empty;
                
            try
            {
                // Handle special case for System process
                if (processName.Equals("System", StringComparison.OrdinalIgnoreCase))
                {
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "kernel32.dll");
                }
                
                // Try to find the process by name
                Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processName));
                if (processes.Length > 0)
                {
                    string path = processes[0].MainModule?.FileName;
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    {
                        return path;
                    }
                }
                
                // Try common locations
                string[] commonLocations = new string[]
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.System),
                    Environment.GetFolderPath(Environment.SpecialFolder.SystemX86),
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
                };
                
                foreach (string location in commonLocations)
                {
                    string path = Path.Combine(location, processName);
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error getting path for {processName}: {ex.Message}", LogLevel.Warning);
            }
            
            return string.Empty;
        }
        
        private Dictionary<string, string> _processIconKeys = new Dictionary<string, string>();
        
        /// <summary>
        /// Gets the process icon key for a process
        /// </summary>
        private string GetProcessIconKey(int processId, string processName)
        {
            // Check if we already have an icon for this process
            string key = $"{processId}_{processName}";
            if (_processIconKeys.TryGetValue(key, out string iconKey))
            {
                return iconKey;
            }
            
            // Check if we have a preloaded icon for this process name
            string processNameLower = Path.GetFileName(processName).ToLower();
            if (imageList.Images.ContainsKey(processNameLower))
            {
                _processIconKeys[key] = processNameLower;
                return processNameLower;
            }
            
            // Special case for System process
            if (processName.Equals("System", StringComparison.OrdinalIgnoreCase))
            {
                if (imageList.Images.ContainsKey("system"))
                {
                    _processIconKeys[key] = "system";
                    return "system";
                }
            }
            
            try
            {
                // Check if this is our own process
                var currentProcess = Process.GetCurrentProcess();
                if (processId == currentProcess.Id)
                {
                    string currentProcessName = Path.GetFileName(currentProcess.MainModule?.FileName).ToLower();
                    if (imageList.Images.ContainsKey(currentProcessName))
                    {
                        _processIconKeys[key] = currentProcessName;
                        return currentProcessName;
                    }
                }
                
                // Try to get the process by ID
                Process process = Process.GetProcessById(processId);
                string processPath = process.MainModule?.FileName;
                
                if (!string.IsNullOrEmpty(processPath) && File.Exists(processPath))
                {
                    // Extract icon from the process executable
                    Icon icon = Icon.ExtractAssociatedIcon(processPath);
                    if (icon != null)
                    {
                        // Add the icon to the image list
                        imageList.Images.Add(key, icon);
                        _processIconKeys[key] = key;
                        return key;
                    }
                }
            }
            catch (Exception ex)
            {
                // Process may have exited or access denied
                Logger.Log($"Error extracting icon for process {processId}: {ex.Message}", LogLevel.Debug);
                
                // Try to get the icon from the process name
                try
                {
                    string path = GetFullPathFromProcessName(processName);
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    {
                        Icon icon = Icon.ExtractAssociatedIcon(path);
                        if (icon != null)
                        {
                            imageList.Images.Add(key, icon);
                            _processIconKeys[key] = key;
                            return key;
                        }
                    }
                }
                catch
                {
                    // Ignore secondary extraction errors
                }
            }
            
            // Use a default icon
            _processIconKeys[key] = "default";
            return "default";
        }
        
        /// <summary>
        /// Handles the applications list view double click event
        /// </summary>
        private void applicationsListView_DoubleClick(object sender, EventArgs e)
        {
            if (applicationsListView.SelectedIndices.Count > 0)
            {
                int index = applicationsListView.SelectedIndices[0];
                if (_processListManager.Applications != null &&
                    index >= 0 && index < _processListManager.Applications.Count)
                {
                    var process = _processListManager.Applications[index];
                    SelectedProcessId = process.Id;
                    SelectedProcessName = process.Name;
                    DialogResult = DialogResult.OK;
                }
            }
        }
        
        /// <summary>
        /// Handles the windows list view double click event
        /// </summary>
        private void windowsListView_DoubleClick(object sender, EventArgs e)
        {
            if (windowsListView.SelectedIndices.Count > 0)
            {
                int index = windowsListView.SelectedIndices[0];
                if (_processListManager.Windows != null &&
                    index >= 0 && index < _processListManager.Windows.Count)
                {
                    var window = _processListManager.Windows[index];
                    SelectedProcessId = window.ProcessId;
                    SelectedProcessName = window.ProcessName;
                    DialogResult = DialogResult.OK;
                }
            }
        }
        
        /// <summary>
        /// Handles the refresh button click event
        /// </summary>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshProcessList();
        }
        
        /// <summary>
        /// Handles the select button click event
        /// </summary>
        private void btnSelect_Click(object sender, EventArgs e)
        {
            // Get the selected process based on the active tab
            switch (tabControl.SelectedIndex)
            {
                case 0: // Applications tab
                    if (applicationsListView.SelectedIndices.Count > 0)
                    {
                        int index = applicationsListView.SelectedIndices[0];
                        if (_processListManager.Applications != null &&
                            index >= 0 && index < _processListManager.Applications.Count)
                        {
                            var process = _processListManager.Applications[index];
                            SelectedProcessId = process.Id;
                            SelectedProcessName = process.Name;
                            DialogResult = DialogResult.OK;
                        }
                    }
                    break;
                    
                case 1: // Processes tab
                    if (processListView.SelectedIndices.Count > 0)
                    {
                        int index = processListView.SelectedIndices[0];
                        if (index >= 0 && index < _processListManager.Processes.Count)
                        {
                            var process = _processListManager.Processes[index];
                            SelectedProcessId = process.Id;
                            SelectedProcessName = process.Name;
                            DialogResult = DialogResult.OK;
                        }
                    }
                    break;
                    
                case 2: // Windows tab
                    if (windowsListView.SelectedIndices.Count > 0)
                    {
                        int index = windowsListView.SelectedIndices[0];
                        if (_processListManager.Windows != null &&
                            index >= 0 && index < _processListManager.Windows.Count)
                        {
                            var window = _processListManager.Windows[index];
                            SelectedProcessId = window.ProcessId;
                            SelectedProcessName = window.ProcessName;
                            DialogResult = DialogResult.OK;
                        }
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Populates a list view with processes in a hierarchical tree structure
        /// </summary>
        /// <param name="listView">The list view to populate</param>
        /// <param name="processes">The collection of processes</param>
        private void PopulateProcessListView(ListView listView, IEnumerable<ProcessInfo> processes)
        {
            try
            {
                // Clear existing items
                listView.BeginUpdate();
                listView.Items.Clear();
                
                // First, organize processes into a parent-child hierarchy
                Dictionary<int, List<ProcessInfo>> processChildren = new Dictionary<int, List<ProcessInfo>>();
                Dictionary<int, ProcessInfo> processById = new Dictionary<int, ProcessInfo>();
                List<ProcessInfo> rootProcesses = new List<ProcessInfo>();
                
                // Build the dictionaries
                foreach (var process in processes)
                {
                    processById[process.Id] = process;
                }
                
                // Build the parent-child relationships
                foreach (var process in processes)
                {
                    if (process.ParentProcessId > 0 && processById.ContainsKey(process.ParentProcessId))
                    {
                        // Add to parent's children list
                        if (!processChildren.ContainsKey(process.ParentProcessId))
                        {
                            processChildren[process.ParentProcessId] = new List<ProcessInfo>();
                        }
                        
                        processChildren[process.ParentProcessId].Add(process);
                    }
                    else
                    {
                        // This is a root process
                        rootProcesses.Add(process);
                    }
                }
                
                // Sort root processes by name
                rootProcesses.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
                
                // Add root processes to the list view
                foreach (var rootProcess in rootProcesses)
                {
                    AddProcessToListView(listView, rootProcess, 0, processChildren);
                }
                
                // Adjust column widths
                listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                listView.EndUpdate();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error populating process list view: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Populates the Windows list view with window processes
        /// </summary>
        /// <param name="listView">The list view to populate</param>
        /// <param name="windows">The collection of window info objects</param>
        private void PopulateWindowsListView(ListView listView, IEnumerable<WindowInfo> windows)
        {
            try
            {
                // Clear existing items
                listView.BeginUpdate();
                listView.Items.Clear();
                
                // Convert WindowInfo objects to ProcessInfo for consistency
                List<ProcessInfo> windowProcesses = new List<ProcessInfo>();
                Dictionary<int, ProcessInfo> processDict = _processListManager.Processes.ToDictionary(p => p.Id);
                
                foreach (var window in windows)
                {
                    bool is64Bit = false;
                    if (processDict.TryGetValue(window.ProcessId, out var proc))
                    {
                        is64Bit = proc.Is64Bit;
                    }
                    
                    windowProcesses.Add(new ProcessInfo
                    {
                        Id = window.ProcessId,
                        Name = window.ProcessName,
                        Is64Bit = is64Bit,
                        WindowTitle = window.Title
                    });
                }
                
                // Use the same process tree logic for windows
                PopulateProcessListView(listView, windowProcesses);
                listView.EndUpdate();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error populating windows list view: {ex.Message}", LogLevel.Error);
            }
        }
        
        // Second ListView_MouseClick method removed to fix duplicate method error
        
        /// <summary>
        /// Adds a process to the list view with proper indentation for hierarchy
        /// </summary>
        /// <param name="listView">The list view</param>
        /// <param name="process">The process to add</param>
        /// <param name="indentLevel">The indentation level</param>
        /// <param name="processChildren">Dictionary of process children</param>
        private void AddProcessToListView(ListView listView, ProcessInfo process, int indentLevel, Dictionary<int, List<ProcessInfo>> processChildren)
        {
            try
            {
                // Create list view item
                ListViewItem item = new ListViewItem();
                
                // Get process icon
                string iconKey = GetProcessIconKey(process.Id, process.Name);
                item.ImageKey = iconKey;
                
                // Add PID
                item.Text = process.Id.ToString();
                
                // Add process name with proper indentation
                string prefix = string.Empty;
                if (indentLevel > 0)
                {
                    prefix = new string(' ', indentLevel * 4);
                }
                
                // Add expand/collapse indicator if the process has children
                bool hasChildren = processChildren.ContainsKey(process.Id) && processChildren[process.Id].Count > 0;
                if (hasChildren)
                {
                    prefix += process.IsExpanded ? "- " : "+ ";
                }
                
                item.SubItems.Add(prefix + process.Name);
                
                // Add architecture information
                string architecture = process.Is64Bit ? "64 bit" : "32 bit";
                item.SubItems.Add(architecture);
                
                // Store the process in the tag
                item.Tag = process;
                
                // Add to list view
                listView.Items.Add(item);
                
                // If expanded, add children
                if (hasChildren && process.IsExpanded)
                {
                    var children = processChildren[process.Id];
                    children.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
                    
                    foreach (var childProcess in children)
                    {
                        AddProcessToListView(listView, childProcess, indentLevel + 1, processChildren);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error adding process to list view: {ex.Message}", LogLevel.Error);
            }
        }
    }
}
