using System;
using System.ComponentModel;
using System.Drawing;
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
            
            // Set up event handlers
            _processListManager.ProcessListRefreshed += ProcessListManager_ProcessListRefreshed;
            
            // Set up filter checkboxes
            showSystemProcessesCheckBox.Checked = _processListManager.ShowSystemProcesses;
            show64BitProcessesCheckBox.Checked = _processListManager.Show64BitProcesses;
            show32BitProcessesCheckBox.Checked = _processListManager.Show32BitProcesses;
            
            // Initial refresh
            RefreshProcessList();
        }
        
        /// <summary>
        /// Handles the form load event
        /// </summary>
        private void ProcessListForm_Load(object sender, EventArgs e)
        {
            // Set up columns
            processListView.Columns.Add("Process", 250);
            processListView.Columns.Add("PID", 80);
            processListView.Columns.Add("Architecture", 100);
            
            // Set up icons
            processImageList.Images.Add("process", SystemIcons.Application);
            processListView.SmallImageList = processImageList;
        }
        
        /// <summary>
        /// Handles the refresh button click event
        /// </summary>
        private async void refreshButton_Click(object sender, EventArgs e)
        {
            await RefreshProcessList();
        }
        
        /// <summary>
        /// Handles the OK button click event
        /// </summary>
        private void okButton_Click(object sender, EventArgs e)
        {
            if (processListView.SelectedIndices.Count > 0)
            {
                int index = processListView.SelectedIndices[0];
                ProcessInfo process = _processListManager.Processes[index];
                
                SelectedProcessId = process.Id;
                SelectedProcessName = process.Name;
                
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a process.", "No Process Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        /// <summary>
        /// Handles the cancel button click event
        /// </summary>
        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        
        /// <summary>
        /// Handles the show system processes checkbox checked changed event
        /// </summary>
        private void showSystemProcessesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _processListManager.ShowSystemProcesses = showSystemProcessesCheckBox.Checked;
        }
        
        /// <summary>
        /// Handles the show 64-bit processes checkbox checked changed event
        /// </summary>
        private void show64BitProcessesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _processListManager.Show64BitProcesses = show64BitProcessesCheckBox.Checked;
        }
        
        /// <summary>
        /// Handles the show 32-bit processes checkbox checked changed event
        /// </summary>
        private void show32BitProcessesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _processListManager.Show32BitProcesses = show32BitProcessesCheckBox.Checked;
        }
        
        /// <summary>
        /// Handles the filter text box text changed event
        /// </summary>
        private void filterTextBox_TextChanged(object sender, EventArgs e)
        {
            // Apply filter
            ApplyFilter();
        }
        
        /// <summary>
        /// Handles the process list view double click event
        /// </summary>
        private void processListView_DoubleClick(object sender, EventArgs e)
        {
            okButton_Click(sender, e);
        }
        
        /// <summary>
        /// Handles the process list view retrieve virtual item event
        /// </summary>
        private void ProcessListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex >= 0 && e.ItemIndex < _processListManager.Processes.Count)
            {
                ProcessInfo process = _processListManager.Processes[e.ItemIndex];
                
                ListViewItem item = new ListViewItem(process.DisplayName);
                item.SubItems.Add(process.Id.ToString());
                item.SubItems.Add(process.Architecture);
                item.ImageKey = "process";
                
                e.Item = item;
            }
        }
        
        /// <summary>
        /// Handles the process list manager process list refreshed event
        /// </summary>
        private void ProcessListManager_ProcessListRefreshed(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ProcessListManager_ProcessListRefreshed(sender, e)));
                return;
            }
            
            // Update list view
            processListView.VirtualListSize = _processListManager.Processes.Count;
            processListView.Invalidate();
            
            // Update status
            statusLabel.Text = $"Found {_processListManager.Processes.Count} processes";
            
            // Enable controls
            refreshButton.Enabled = true;
            _isRefreshing = false;
        }
        
        /// <summary>
        /// Refreshes the process list
        /// </summary>
        private async Task RefreshProcessList()
        {
            if (_isRefreshing)
            {
                return;
            }
            
            _isRefreshing = true;
            refreshButton.Enabled = false;
            statusLabel.Text = "Refreshing process list...";
            
            await _processListManager.RefreshProcessListAsync();
        }
        
        /// <summary>
        /// Applies the filter to the process list
        /// </summary>
        private void ApplyFilter()
        {
            // TODO: Implement filtering
        }
        
        #region Designer Generated Code
        
        private System.ComponentModel.IContainer components = null;
        private ListView processListView;
        private Button refreshButton;
        private Button okButton;
        private Button cancelButton;
        private CheckBox showSystemProcessesCheckBox;
        private CheckBox show64BitProcessesCheckBox;
        private CheckBox show32BitProcessesCheckBox;
        private TextBox filterTextBox;
        private Label filterLabel;
        private Label statusLabel;
        private ImageList processImageList;
        
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.processListView = new System.Windows.Forms.ListView();
            this.refreshButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.showSystemProcessesCheckBox = new System.Windows.Forms.CheckBox();
            this.show64BitProcessesCheckBox = new System.Windows.Forms.CheckBox();
            this.show32BitProcessesCheckBox = new System.Windows.Forms.CheckBox();
            this.filterTextBox = new System.Windows.Forms.TextBox();
            this.filterLabel = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.processImageList = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // processListView
            // 
            this.processListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.processListView.FullRowSelect = true;
            this.processListView.HideSelection = false;
            this.processListView.Location = new System.Drawing.Point(12, 41);
            this.processListView.MultiSelect = false;
            this.processListView.Name = "processListView";
            this.processListView.Size = new System.Drawing.Size(560, 300);
            this.processListView.TabIndex = 0;
            this.processListView.UseCompatibleStateImageBehavior = false;
            this.processListView.View = System.Windows.Forms.View.Details;
            this.processListView.DoubleClick += new System.EventHandler(this.processListView_DoubleClick);
            // 
            // refreshButton
            // 
            this.refreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.refreshButton.Location = new System.Drawing.Point(12, 376);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(75, 23);
            this.refreshButton.TabIndex = 1;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(416, 376);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(497, 376);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // showSystemProcessesCheckBox
            // 
            this.showSystemProcessesCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.showSystemProcessesCheckBox.AutoSize = true;
            this.showSystemProcessesCheckBox.Location = new System.Drawing.Point(12, 347);
            this.showSystemProcessesCheckBox.Name = "showSystemProcessesCheckBox";
            this.showSystemProcessesCheckBox.Size = new System.Drawing.Size(142, 17);
            this.showSystemProcessesCheckBox.TabIndex = 4;
            this.showSystemProcessesCheckBox.Text = "Show System Processes";
            this.showSystemProcessesCheckBox.UseVisualStyleBackColor = true;
            this.showSystemProcessesCheckBox.CheckedChanged += new System.EventHandler(this.showSystemProcessesCheckBox_CheckedChanged);
            // 
            // show64BitProcessesCheckBox
            // 
            this.show64BitProcessesCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.show64BitProcessesCheckBox.AutoSize = true;
            this.show64BitProcessesCheckBox.Location = new System.Drawing.Point(160, 347);
            this.show64BitProcessesCheckBox.Name = "show64BitProcessesCheckBox";
            this.show64BitProcessesCheckBox.Size = new System.Drawing.Size(135, 17);
            this.show64BitProcessesCheckBox.TabIndex = 5;
            this.show64BitProcessesCheckBox.Text = "Show 64-bit Processes";
            this.show64BitProcessesCheckBox.UseVisualStyleBackColor = true;
            this.show64BitProcessesCheckBox.CheckedChanged += new System.EventHandler(this.show64BitProcessesCheckBox_CheckedChanged);
            // 
            // show32BitProcessesCheckBox
            // 
            this.show32BitProcessesCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.show32BitProcessesCheckBox.AutoSize = true;
            this.show32BitProcessesCheckBox.Location = new System.Drawing.Point(301, 347);
            this.show32BitProcessesCheckBox.Name = "show32BitProcessesCheckBox";
            this.show32BitProcessesCheckBox.Size = new System.Drawing.Size(135, 17);
            this.show32BitProcessesCheckBox.TabIndex = 6;
            this.show32BitProcessesCheckBox.Text = "Show 32-bit Processes";
            this.show32BitProcessesCheckBox.UseVisualStyleBackColor = true;
            this.show32BitProcessesCheckBox.CheckedChanged += new System.EventHandler(this.show32BitProcessesCheckBox_CheckedChanged);
            // 
            // filterTextBox
            // 
            this.filterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filterTextBox.Location = new System.Drawing.Point(50, 12);
            this.filterTextBox.Name = "filterTextBox";
            this.filterTextBox.Size = new System.Drawing.Size(522, 20);
            this.filterTextBox.TabIndex = 7;
            this.filterTextBox.TextChanged += new System.EventHandler(this.filterTextBox_TextChanged);
            // 
            // filterLabel
            // 
            this.filterLabel.AutoSize = true;
            this.filterLabel.Location = new System.Drawing.Point(12, 15);
            this.filterLabel.Name = "filterLabel";
            this.filterLabel.Size = new System.Drawing.Size(32, 13);
            this.filterLabel.TabIndex = 8;
            this.filterLabel.Text = "Filter:";
            // 
            // statusLabel
            // 
            this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusLabel.Location = new System.Drawing.Point(93, 381);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(317, 13);
            this.statusLabel.TabIndex = 9;
            this.statusLabel.Text = "Ready";
            // 
            // processImageList
            // 
            this.processImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.processImageList.ImageSize = new System.Drawing.Size(16, 16);
            this.processImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // ProcessListForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(584, 411);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.filterLabel);
            this.Controls.Add(this.filterTextBox);
            this.Controls.Add(this.show32BitProcessesCheckBox);
            this.Controls.Add(this.show64BitProcessesCheckBox);
            this.Controls.Add(this.showSystemProcessesCheckBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.processListView);
            this.MinimumSize = new System.Drawing.Size(400, 300);
            this.Name = "ProcessListForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Process List";
            this.Load += new System.EventHandler(this.ProcessListForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        
        #endregion
    }
}
