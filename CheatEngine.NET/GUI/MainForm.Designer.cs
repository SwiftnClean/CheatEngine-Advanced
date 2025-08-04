namespace CheatEngine.NET.GUI
{
    partial class MainForm
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
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openProcessToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.openTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.memoryViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disassemblerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pointerScannerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoAssemblerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.panelScanControls = new System.Windows.Forms.Panel();
            this.rainbowPcButton = new CheatEngine.NET.GUI.RainbowPcButton();
            this.btnNextScan = new System.Windows.Forms.Button();
            this.btnScan = new System.Windows.Forms.Button();
            this.txtScanValue = new System.Windows.Forms.TextBox();
            this.lblScanValue = new System.Windows.Forms.Label();
            this.cmbScanType = new System.Windows.Forms.ComboBox();
            this.lblScanType = new System.Windows.Forms.Label();
            this.progressBarScan = new System.Windows.Forms.ProgressBar();
            this.lblSelectedProcess = new System.Windows.Forms.Label();
            this.dgvAddressList = new System.Windows.Forms.DataGridView();
            this.colAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.panelScanControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAddressList)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(800, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openProcessToolStripMenuItem,
            this.toolStripSeparator1,
            this.openTableToolStripMenuItem,
            this.saveTableToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openProcessToolStripMenuItem
            // 
            this.openProcessToolStripMenuItem.Name = "openProcessToolStripMenuItem";
            this.openProcessToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openProcessToolStripMenuItem.Text = "Open Process...";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // openTableToolStripMenuItem
            // 
            this.openTableToolStripMenuItem.Name = "openTableToolStripMenuItem";
            this.openTableToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openTableToolStripMenuItem.Text = "Open Table...";
            // 
            // saveTableToolStripMenuItem
            // 
            this.saveTableToolStripMenuItem.Name = "saveTableToolStripMenuItem";
            this.saveTableToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveTableToolStripMenuItem.Text = "Save Table...";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripSeparator3,
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.undoToolStripMenuItem.Text = "Undo";
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.redoToolStripMenuItem.Text = "Redo";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(177, 6);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.cutToolStripMenuItem.Text = "Cut";
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.memoryViewerToolStripMenuItem,
            this.disassemblerToolStripMenuItem,
            this.pointerScannerToolStripMenuItem,
            this.autoAssemblerToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // memoryViewerToolStripMenuItem
            // 
            this.memoryViewerToolStripMenuItem.Name = "memoryViewerToolStripMenuItem";
            this.memoryViewerToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.memoryViewerToolStripMenuItem.Text = "Memory Viewer";
            // 
            // disassemblerToolStripMenuItem
            // 
            this.disassemblerToolStripMenuItem.Name = "disassemblerToolStripMenuItem";
            this.disassemblerToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.disassemblerToolStripMenuItem.Text = "Disassembler";
            // 
            // pointerScannerToolStripMenuItem
            // 
            this.pointerScannerToolStripMenuItem.Name = "pointerScannerToolStripMenuItem";
            this.pointerScannerToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.pointerScannerToolStripMenuItem.Text = "Pointer Scanner";
            // 
            // autoAssemblerToolStripMenuItem
            // 
            this.autoAssemblerToolStripMenuItem.Name = "autoAssemblerToolStripMenuItem";
            this.autoAssemblerToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.autoAssemblerToolStripMenuItem.Text = "Auto Assembler";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 428);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(800, 22);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(39, 17);
            this.statusLabel.Text = "Ready";
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 24);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.panelScanControls);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.dgvAddressList);
            this.splitContainer.Size = new System.Drawing.Size(800, 404);
            this.splitContainer.SplitterDistance = 200;
            this.splitContainer.TabIndex = 2;
            // 
            // panelScanControls
            // 
            this.panelScanControls.Controls.Add(this.rainbowPcButton);
            this.panelScanControls.Controls.Add(this.btnNextScan);
            this.panelScanControls.Controls.Add(this.btnScan);
            this.panelScanControls.Controls.Add(this.txtScanValue);
            this.panelScanControls.Controls.Add(this.lblScanValue);
            this.panelScanControls.Controls.Add(this.cmbScanType);
            this.panelScanControls.Controls.Add(this.lblScanType);
            this.panelScanControls.Controls.Add(this.progressBarScan);
            this.panelScanControls.Controls.Add(this.lblSelectedProcess);
            this.panelScanControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelScanControls.Location = new System.Drawing.Point(0, 0);
            this.panelScanControls.Name = "panelScanControls";
            this.panelScanControls.Size = new System.Drawing.Size(800, 200);
            this.panelScanControls.TabIndex = 0;
            // 
            // rainbowPcButton
            // 
            this.rainbowPcButton.BackColor = System.Drawing.Color.Black;
            this.rainbowPcButton.ForeColor = System.Drawing.Color.White;
            this.rainbowPcButton.Location = new System.Drawing.Point(15, 15);
            this.rainbowPcButton.Name = "rainbowPcButton";
            this.rainbowPcButton.Size = new System.Drawing.Size(40, 23);
            this.rainbowPcButton.TabIndex = 8;
            this.rainbowPcButton.Click += new System.EventHandler(this.rainbowPcButton_Click);
            // 
            // btnNextScan
            // 
            this.btnNextScan.Enabled = false;
            this.btnNextScan.Location = new System.Drawing.Point(174, 124);
            this.btnNextScan.Name = "btnNextScan";
            this.btnNextScan.Size = new System.Drawing.Size(75, 23);
            this.btnNextScan.TabIndex = 7;
            this.btnNextScan.Text = "Next Scan";
            this.btnNextScan.UseVisualStyleBackColor = true;
            this.btnNextScan.Click += new System.EventHandler(this.btnNextScan_Click);
            // 
            // btnScan
            // 
            this.btnScan.Enabled = false;
            this.btnScan.Location = new System.Drawing.Point(93, 124);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(75, 23);
            this.btnScan.TabIndex = 6;
            this.btnScan.Text = "New Scan";
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // txtScanValue
            // 
            this.txtScanValue.Enabled = false;
            this.txtScanValue.Location = new System.Drawing.Point(93, 95);
            this.txtScanValue.Name = "txtScanValue";
            this.txtScanValue.Size = new System.Drawing.Size(156, 23);
            this.txtScanValue.TabIndex = 5;
            // 
            // lblScanValue
            // 
            this.lblScanValue.AutoSize = true;
            this.lblScanValue.Location = new System.Drawing.Point(12, 98);
            this.lblScanValue.Name = "lblScanValue";
            this.lblScanValue.Size = new System.Drawing.Size(38, 15);
            this.lblScanValue.TabIndex = 4;
            this.lblScanValue.Text = "Value:";
            // 
            // cmbScanType
            // 
            this.cmbScanType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScanType.Enabled = false;
            this.cmbScanType.FormattingEnabled = true;
            this.cmbScanType.Location = new System.Drawing.Point(93, 66);
            this.cmbScanType.Name = "cmbScanType";
            this.cmbScanType.Size = new System.Drawing.Size(156, 23);
            this.cmbScanType.TabIndex = 3;
            // 
            // lblScanType
            // 
            this.lblScanType.AutoSize = true;
            this.lblScanType.Location = new System.Drawing.Point(12, 69);
            this.lblScanType.Name = "lblScanType";
            this.lblScanType.Size = new System.Drawing.Size(62, 15);
            this.lblScanType.TabIndex = 2;
            this.lblScanType.Text = "Scan Type:";
            // 
            // progressBarScan
            // 
            this.progressBarScan.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarScan.Location = new System.Drawing.Point(12, 165);
            this.progressBarScan.Name = "progressBarScan";
            this.progressBarScan.Size = new System.Drawing.Size(776, 23);
            this.progressBarScan.TabIndex = 1;
            // 
            // lblSelectedProcess
            // 
            this.lblSelectedProcess.AutoSize = true;
            this.lblSelectedProcess.Location = new System.Drawing.Point(171, 20);
            this.lblSelectedProcess.Name = "lblSelectedProcess";
            this.lblSelectedProcess.Size = new System.Drawing.Size(131, 15);
            this.lblSelectedProcess.TabIndex = 0;
            this.lblSelectedProcess.Text = "No process selected yet";
            // 
            // dgvAddressList
            // 
            this.dgvAddressList.AllowUserToAddRows = false;
            this.dgvAddressList.AllowUserToDeleteRows = false;
            this.dgvAddressList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAddressList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colAddress,
            this.colType,
            this.colValue,
            this.colDescription});
            this.dgvAddressList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAddressList.Location = new System.Drawing.Point(0, 0);
            this.dgvAddressList.Name = "dgvAddressList";
            this.dgvAddressList.ReadOnly = true;
            this.dgvAddressList.RowTemplate.Height = 25;
            this.dgvAddressList.Size = new System.Drawing.Size(800, 200);
            this.dgvAddressList.TabIndex = 0;
            // 
            // colAddress
            // 
            this.colAddress.HeaderText = "Address";
            this.colAddress.Name = "colAddress";
            this.colAddress.ReadOnly = true;
            // 
            // colType
            // 
            this.colType.HeaderText = "Type";
            this.colType.Name = "colType";
            this.colType.ReadOnly = true;
            // 
            // colValue
            // 
            this.colValue.HeaderText = "Value";
            this.colValue.Name = "colValue";
            this.colValue.ReadOnly = true;
            // 
            // colDescription
            // 
            this.colDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDescription.HeaderText = "Description";
            this.colDescription.Name = "colDescription";
            this.colDescription.ReadOnly = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.Text = "Cheat Engine .NET";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.panelScanControls.ResumeLayout(false);
            this.panelScanControls.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAddressList)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openProcessToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem openTableToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveTableToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem memoryViewerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disassemblerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pointerScannerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoAssemblerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Panel panelScanControls;
        private CheatEngine.NET.GUI.RainbowPcButton rainbowPcButton;
        private System.Windows.Forms.Button btnNextScan;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.TextBox txtScanValue;
        private System.Windows.Forms.Label lblScanValue;
        private System.Windows.Forms.ComboBox cmbScanType;
        private System.Windows.Forms.Label lblScanType;
        private System.Windows.Forms.ProgressBar progressBarScan;
        private System.Windows.Forms.Label lblSelectedProcess;
        private System.Windows.Forms.DataGridView dgvAddressList;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAddress;
        private System.Windows.Forms.DataGridViewTextBoxColumn colType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescription;
    }
}
