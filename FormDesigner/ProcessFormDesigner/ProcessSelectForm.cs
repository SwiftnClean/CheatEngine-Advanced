using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcessFormDesigner;

public partial class ProcessSelectForm : Form
{
    // Process info class to hold process data
    public class ProcessInfo
    {
        public int ProcessId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Architecture { get; set; } = string.Empty;
        public bool IsSystemProcess { get; set; }
        public bool HasWindow { get; set; }
        public Icon? Icon { get; set; }
    }

    // Process categories
    public enum ProcessCategory
    {
        Apps,
        Background,
        Windows
    }

    // Selected process ID
    private int _selectedProcessId = -1;
    private List<ProcessInfo> _allProcesses = new List<ProcessInfo>();
    private List<ProcessInfo> _filteredProcesses = new List<ProcessInfo>();
    private string _filterText = string.Empty;

    public int SelectedProcessId => _selectedProcessId;

    public ProcessSelectForm()
    {
        InitializeComponent();
    }

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
    /// Initializes the image list for process icons
    /// </summary>
    private void InitializeImageList()
    {
        imageList.Images.Clear();
        imageList.ColorDepth = ColorDepth.Depth32Bit;
        imageList.ImageSize = new Size(24, 24); // Larger icons for better visibility
        imageList.TransparentColor = Color.Transparent;
    }
    
    /// <summary>
    /// Fills the process list with running processes
    /// </summary>
    private void FillProcessList()
    {
        try
        {
            Cursor = Cursors.WaitCursor;
            _allProcesses.Clear();
            
            // Get all processes
            Process[] processes = Process.GetProcesses();
            
            foreach (Process process in processes)
            {
                try
                {
                    ProcessInfo processInfo = new ProcessInfo
                    {
                        ProcessId = process.Id,
                        Name = process.ProcessName,
                        Architecture = Environment.Is64BitOperatingSystem ? "x64" : "x86", // Simplified for demo
                        IsSystemProcess = IsSystemProcess(process),
                        HasWindow = process.MainWindowHandle != IntPtr.Zero
                    };
                    
                    try
                    {
                        processInfo.Icon = Icon.ExtractAssociatedIcon(process.MainModule?.FileName ?? string.Empty);
                    }
                    catch
                    {
                        // Use default icon if extraction fails
                    }
                    
                    _allProcesses.Add(processInfo);
                }
                catch
                {
                    // Skip processes that cannot be accessed
                }
            }
            
            // Apply filtering
            ApplyFiltering();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error filling process list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }
    
    /// <summary>
    /// Determines if a process is a system process
    /// </summary>
    private bool IsSystemProcess(Process process)
    {
        // Simplified check for demo purposes
        string[] systemProcessNames = { "svchost", "csrss", "smss", "wininit", "services", "lsass", "winlogon" };
        return systemProcessNames.Contains(process.ProcessName.ToLower());
    }
    
    /// <summary>
    /// Applies filtering to the process list based on filter text and category
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
                bool matchesFilter = string.IsNullOrEmpty(_filterText) ||
                                    process.Name.Contains(_filterText, StringComparison.OrdinalIgnoreCase) ||
                                    process.ProcessId.ToString().Contains(_filterText) ||
                                    process.Architecture.Contains(_filterText, StringComparison.OrdinalIgnoreCase);
                
                if (matchesFilter)
                {
                    _filteredProcesses.Add(process);
                    
                    // Create list view item
                    ListViewItem item = new ListViewItem(process.ProcessId.ToString());
                    item.SubItems.Add(process.Architecture);
                    item.SubItems.Add(process.Name);
                    item.Tag = process;
                    
                    // Add icon
                    if (process.Icon != null)
                    {
                        if (!imageList.Images.ContainsKey(process.ProcessId.ToString()))
                        {
                            imageList.Images.Add(process.ProcessId.ToString(), process.Icon);
                        }
                        item.ImageKey = process.ProcessId.ToString();
                    }
                    
                    // Assign to appropriate group
                    if (process.HasWindow && !process.IsSystemProcess)
                    {
                        item.Group = processListView.Groups[0]; // Process Apps
                    }
                    else if (!process.HasWindow && !process.IsSystemProcess)
                    {
                        item.Group = processListView.Groups[1]; // Background Processes
                    }
                    else
                    {
                        item.Group = processListView.Groups[2]; // Windows Processes
                    }
                    
                    processListView.Items.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error applying filter: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        }
        else
        {
            _selectedProcessId = -1;
        }
        
        btnOK.Enabled = _selectedProcessId != -1;
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
}
