namespace CheatEngine.NET.GUI
{
    partial class ProcessListForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            // Removed resource manager initialization to fix resource loading error
            this.processListView = new System.Windows.Forms.ListView();
            this.columnHeaderIcon = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderPID = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderName = new System.Windows.Forms.ColumnHeader();
            this.processTreeTabPage = new System.Windows.Forms.TabPage();
            this.processTreeView = new System.Windows.Forms.TreeView();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnSelect = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.filterGroupBox = new System.Windows.Forms.GroupBox();
            this.show32BitProcessesCheckBox = new System.Windows.Forms.CheckBox();
            this.show64BitProcessesCheckBox = new System.Windows.Forms.CheckBox();
            this.showSystemProcessesCheckBox = new System.Windows.Forms.CheckBox();
            this.txtFilter = new System.Windows.Forms.TextBox();
            this.lblFilter = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.applicationsTabPage = new System.Windows.Forms.TabPage();
            this.applicationsListView = new System.Windows.Forms.ListView();
            this.columnHeaderAppIcon = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderAppPID = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderAppName = new System.Windows.Forms.ColumnHeader();
            this.processesTabPage = new System.Windows.Forms.TabPage();
            this.windowsTabPage = new System.Windows.Forms.TabPage();
            this.windowsListView = new System.Windows.Forms.ListView();
            this.columnHeaderWinIcon = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderWinPID = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderWinTitle = new System.Windows.Forms.ColumnHeader();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.filterGroupBox.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.applicationsTabPage.SuspendLayout();
            this.processesTabPage.SuspendLayout();
            this.windowsTabPage.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // processListView
            // 
            this.processListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderIcon,
            this.columnHeaderPID,
            this.columnHeaderName});
            this.processListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.processListView.FullRowSelect = true;
            this.processListView.HideSelection = false;
            this.processListView.Location = new System.Drawing.Point(3, 3);
            this.processListView.MultiSelect = false;
            this.processListView.Name = "processListView";
            this.processListView.Size = new System.Drawing.Size(586, 292);
            this.processListView.SmallImageList = this.imageList;
            this.processListView.TabIndex = 0;
            this.processListView.UseCompatibleStateImageBehavior = false;
            this.processListView.View = System.Windows.Forms.View.Details;
            this.processListView.VirtualMode = true;
            this.processListView.DoubleClick += new System.EventHandler(this.processListView_DoubleClick);
            // 
            // columnHeaderIcon
            // 
            this.columnHeaderIcon.Text = "";
            this.columnHeaderIcon.Width = 20;
            // 
            // columnHeaderPID
            // 
            this.columnHeaderPID.Text = "PID";
            this.columnHeaderPID.Width = 80;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 300;
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRefresh.Location = new System.Drawing.Point(12, 415);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnSelect
            // 
            this.btnSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelect.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSelect.Location = new System.Drawing.Point(441, 415);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(75, 23);
            this.btnSelect.TabIndex = 2;
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(522, 415);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // filterGroupBox
            // 
            this.filterGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filterGroupBox.Controls.Add(this.show32BitProcessesCheckBox);
            this.filterGroupBox.Controls.Add(this.show64BitProcessesCheckBox);
            this.filterGroupBox.Controls.Add(this.showSystemProcessesCheckBox);
            this.filterGroupBox.Controls.Add(this.txtFilter);
            this.filterGroupBox.Controls.Add(this.lblFilter);
            this.filterGroupBox.Location = new System.Drawing.Point(12, 12);
            this.filterGroupBox.Name = "filterGroupBox";
            this.filterGroupBox.Size = new System.Drawing.Size(585, 59);
            this.filterGroupBox.TabIndex = 4;
            this.filterGroupBox.TabStop = false;
            this.filterGroupBox.Text = "Filter";
            // 
            // show32BitProcessesCheckBox
            // 
            this.show32BitProcessesCheckBox.AutoSize = true;
            this.show32BitProcessesCheckBox.Checked = true;
            this.show32BitProcessesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.show32BitProcessesCheckBox.Location = new System.Drawing.Point(493, 25);
            this.show32BitProcessesCheckBox.Name = "show32BitProcessesCheckBox";
            this.show32BitProcessesCheckBox.Size = new System.Drawing.Size(86, 19);
            this.show32BitProcessesCheckBox.TabIndex = 4;
            this.show32BitProcessesCheckBox.Text = "32-bit Procs";
            this.show32BitProcessesCheckBox.UseVisualStyleBackColor = true;
            this.show32BitProcessesCheckBox.CheckedChanged += new System.EventHandler(this.show32BitProcessesCheckBox_CheckedChanged);
            // 
            // show64BitProcessesCheckBox
            // 
            this.show64BitProcessesCheckBox.AutoSize = true;
            this.show64BitProcessesCheckBox.Checked = true;
            this.show64BitProcessesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.show64BitProcessesCheckBox.Location = new System.Drawing.Point(401, 25);
            this.show64BitProcessesCheckBox.Name = "show64BitProcessesCheckBox";
            this.show64BitProcessesCheckBox.Size = new System.Drawing.Size(86, 19);
            this.show64BitProcessesCheckBox.TabIndex = 3;
            this.show64BitProcessesCheckBox.Text = "64-bit Procs";
            this.show64BitProcessesCheckBox.UseVisualStyleBackColor = true;
            this.show64BitProcessesCheckBox.CheckedChanged += new System.EventHandler(this.show64BitProcessesCheckBox_CheckedChanged);
            // 
            // showSystemProcessesCheckBox
            // 
            this.showSystemProcessesCheckBox.AutoSize = true;
            this.showSystemProcessesCheckBox.Location = new System.Drawing.Point(293, 25);
            this.showSystemProcessesCheckBox.Name = "showSystemProcessesCheckBox";
            this.showSystemProcessesCheckBox.Size = new System.Drawing.Size(102, 19);
            this.showSystemProcessesCheckBox.TabIndex = 2;
            this.showSystemProcessesCheckBox.Text = "System Procs";
            this.showSystemProcessesCheckBox.UseVisualStyleBackColor = true;
            this.showSystemProcessesCheckBox.CheckedChanged += new System.EventHandler(this.showSystemProcessesCheckBox_CheckedChanged);
            // 
            // txtFilter
            // 
            this.txtFilter.Location = new System.Drawing.Point(73, 23);
            this.txtFilter.Name = "txtFilter";
            this.txtFilter.Size = new System.Drawing.Size(214, 23);
            this.txtFilter.TabIndex = 1;
            this.txtFilter.TextChanged += new System.EventHandler(this.txtFilter_TextChanged);
            // 
            // lblFilter
            // 
            this.lblFilter.AutoSize = true;
            this.lblFilter.Location = new System.Drawing.Point(6, 26);
            this.lblFilter.Name = "lblFilter";
            this.lblFilter.Size = new System.Drawing.Size(61, 15);
            this.lblFilter.TabIndex = 0;
            this.lblFilter.Text = "Filter Text:";
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.applicationsTabPage);
            this.tabControl.Controls.Add(this.processesTabPage);
            this.tabControl.Controls.Add(this.windowsTabPage);
            this.tabControl.Controls.Add(this.processTreeTabPage);
            this.tabControl.Location = new System.Drawing.Point(12, 77);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(600, 332);
            this.tabControl.TabIndex = 5;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // applicationsTabPage
            // 
            this.applicationsTabPage.Controls.Add(this.applicationsListView);
            this.applicationsTabPage.Location = new System.Drawing.Point(4, 24);
            this.applicationsTabPage.Name = "applicationsTabPage";
            this.applicationsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.applicationsTabPage.Size = new System.Drawing.Size(592, 304);
            this.applicationsTabPage.TabIndex = 0;
            this.applicationsTabPage.Text = "Process Apps";
            this.applicationsTabPage.UseVisualStyleBackColor = true;
            // 
            // applicationsListView
            // 
            this.applicationsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderAppIcon,
            this.columnHeaderAppPID,
            this.columnHeaderAppName});
            this.applicationsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.applicationsListView.FullRowSelect = true;
            this.applicationsListView.HideSelection = false;
            this.applicationsListView.Location = new System.Drawing.Point(3, 3);
            this.applicationsListView.MultiSelect = false;
            this.applicationsListView.Name = "applicationsListView";
            this.applicationsListView.Size = new System.Drawing.Size(586, 298);
            this.applicationsListView.SmallImageList = this.imageList;
            this.applicationsListView.TabIndex = 1;
            this.applicationsListView.UseCompatibleStateImageBehavior = false;
            this.applicationsListView.View = System.Windows.Forms.View.Details;
            this.applicationsListView.VirtualMode = true;
            this.applicationsListView.DoubleClick += new System.EventHandler(this.applicationsListView_DoubleClick);
            // 
            // columnHeaderAppIcon
            // 
            this.columnHeaderAppIcon.Text = "";
            this.columnHeaderAppIcon.Width = 20;
            // 
            // columnHeaderAppPID
            // 
            this.columnHeaderAppPID.Text = "PID";
            this.columnHeaderAppPID.Width = 80;
            // 
            // columnHeaderAppName
            // 
            this.columnHeaderAppName.Text = "Name";
            this.columnHeaderAppName.Width = 300;
            // 
            // processesTabPage
            // 
            this.processesTabPage.Controls.Add(this.processListView);
            this.processesTabPage.Location = new System.Drawing.Point(4, 24);
            this.processesTabPage.Name = "processesTabPage";
            this.processesTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.processesTabPage.Size = new System.Drawing.Size(592, 304);
            this.processesTabPage.TabIndex = 1;
            this.processesTabPage.Text = "Background Processes";
            this.processesTabPage.UseVisualStyleBackColor = true;
            // 
            // windowsTabPage
            // 
            this.windowsTabPage.Controls.Add(this.windowsListView);
            this.windowsTabPage.Location = new System.Drawing.Point(4, 24);
            this.windowsTabPage.Name = "windowsTabPage";
            this.windowsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.windowsTabPage.Size = new System.Drawing.Size(592, 304);
            this.windowsTabPage.TabIndex = 2;
            this.windowsTabPage.Text = "Windows Processes";
            this.windowsTabPage.UseVisualStyleBackColor = true;
            // 
            // windowsListView
            // 
            this.windowsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderWinIcon,
            this.columnHeaderWinPID,
            this.columnHeaderWinTitle});
            this.windowsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.windowsListView.FullRowSelect = true;
            this.windowsListView.HideSelection = false;
            this.windowsListView.Location = new System.Drawing.Point(3, 3);
            this.windowsListView.MultiSelect = false;
            this.windowsListView.Name = "windowsListView";
            this.windowsListView.Size = new System.Drawing.Size(586, 298);
            this.windowsListView.SmallImageList = this.imageList;
            this.windowsListView.TabIndex = 1;
            this.windowsListView.UseCompatibleStateImageBehavior = false;
            this.windowsListView.View = System.Windows.Forms.View.Details;
            this.windowsListView.VirtualMode = true;
            this.windowsListView.DoubleClick += new System.EventHandler(this.windowsListView_DoubleClick);
            // 
            // columnHeaderWinIcon
            // 
            this.columnHeaderWinIcon.Text = "";
            this.columnHeaderWinIcon.Width = 20;
            // 
            // columnHeaderWinPID
            // 
            this.columnHeaderWinPID.Text = "PID";
            this.columnHeaderWinPID.Width = 80;
            // 
            // columnHeaderWinTitle
            // 
            this.columnHeaderWinTitle.Text = "Title";
            this.columnHeaderWinTitle.Width = 300;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 450);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(624, 22);
            this.statusStrip.TabIndex = 6;
            this.statusStrip.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(39, 17);
            this.statusLabel.Text = "Ready";
            // 
            // ProcessListForm
            // 
            this.AcceptButton = this.btnSelect;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(624, 472);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.filterGroupBox);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.btnRefresh);
            // Removed icon reference that was causing resource loading errors
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "ProcessListForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Process List";
            this.Load += new System.EventHandler(this.ProcessListForm_Load);
            this.filterGroupBox.ResumeLayout(false);
            this.filterGroupBox.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.applicationsTabPage.ResumeLayout(false);
            this.processesTabPage.ResumeLayout(false);
            this.windowsTabPage.ResumeLayout(false);
            this.processTreeTabPage.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ListView processListView;
        private ColumnHeader columnHeaderIcon;
        private ColumnHeader columnHeaderPID;
        private ColumnHeader columnHeaderName;
        private ImageList imageList;
        private Button btnRefresh;
        private Button btnSelect;
        private Button btnCancel;
        private GroupBox filterGroupBox;
        private CheckBox show32BitProcessesCheckBox;
        private CheckBox show64BitProcessesCheckBox;
        private CheckBox showSystemProcessesCheckBox;
        private TextBox txtFilter;
        private Label lblFilter;
        private TabControl tabControl;
        private TabPage applicationsTabPage;
        private TabPage processesTabPage;
        private TabPage windowsTabPage;
        private TabPage processTreeTabPage;
        private ListView applicationsListView;
        private ColumnHeader columnHeaderAppIcon;
        private ColumnHeader columnHeaderAppPID;
        private ColumnHeader columnHeaderAppName;
        private ListView windowsListView;
        private ColumnHeader columnHeaderWinIcon;
        private ColumnHeader columnHeaderWinPID;
        private ColumnHeader columnHeaderWinTitle;
        private TreeView processTreeView;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
    }
}
