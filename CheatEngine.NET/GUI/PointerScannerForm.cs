using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CheatEngine.NET.Core;
using CheatEngine.NET.Memory;
using CheatEngine.NET.Scanner;
using CheatEngine.NET.Utils;

namespace CheatEngine.NET.GUI
{
    /// <summary>
    /// Form for scanning for pointers to a target address
    /// </summary>
    public partial class PointerScannerForm : Form
    {
        private readonly PointerScanner _pointerScanner;
        private bool _isScanning = false;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PointerScannerForm"/> class
        /// </summary>
        public PointerScannerForm()
        {
            InitializeComponent();
            
            // Create pointer scanner
            _pointerScanner = new PointerScanner();
            
            // Set up event handlers
            _pointerScanner.PointerScanProgress += PointerScanner_PointerScanProgress;
            _pointerScanner.PointerScanComplete += PointerScanner_PointerScanComplete;
        }
        
        /// <summary>
        /// Handles the form load event
        /// </summary>
        private void PointerScannerForm_Load(object sender, EventArgs e)
        {
            // Check if a process is attached
            if (CheatEngineCore.TargetProcess == null)
            {
                MessageBox.Show("No process is attached. Please attach to a process first.", "No Process", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }
            
            // Set up max level combo box
            for (int i = 1; i <= 8; i++)
            {
                maxLevelComboBox.Items.Add(i);
            }
            maxLevelComboBox.SelectedIndex = 2; // Default to level 3
            
            // Set up pointer results list view
            pointerResultsListView.View = View.Details;
            pointerResultsListView.FullRowSelect = true;
            pointerResultsListView.GridLines = true;
            pointerResultsListView.Columns.Add("Base Address", 120);
            pointerResultsListView.Columns.Add("Pointer Path", 300);
            pointerResultsListView.Columns.Add("Final Address", 120);
        }
        
        /// <summary>
        /// Handles the scan button click event
        /// </summary>
        private async void scanButton_Click(object sender, EventArgs e)
        {
            if (_isScanning)
            {
                // Stop scan
                _pointerScanner.StopScan();
                return;
            }
            
            // Validate target address
            if (!TryParseAddress(targetAddressTextBox.Text, out IntPtr targetAddress))
            {
                MessageBox.Show("Invalid target address. Please enter a valid hexadecimal address.", "Invalid Address", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Get max level
            int maxLevel = (int)maxLevelComboBox.SelectedItem;
            
            // Start scan
            _isScanning = true;
            scanButton.Text = "Stop";
            statusLabel.Text = "Scanning for pointers...";
            progressBar.Value = 0;
            pointerResultsListView.Items.Clear();
            
            try
            {
                await _pointerScanner.ScanForPointersAsync(targetAddress, maxLevel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error scanning for pointers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isScanning = false;
                scanButton.Text = "Scan";
                statusLabel.Text = "Scan failed.";
            }
        }
        
        /// <summary>
        /// Handles the add to address list button click event
        /// </summary>
        private void addToAddressListButton_Click(object sender, EventArgs e)
        {
            // Check if a pointer is selected
            if (pointerResultsListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a pointer to add to the address list.", "No Pointer Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Get selected pointer
            ListViewItem item = pointerResultsListView.SelectedItems[0];
            string baseAddress = item.SubItems[0].Text;
            string pointerPath = item.SubItems[1].Text;
            string finalAddress = item.SubItems[2].Text;
            
            // Add to address list
            // TODO: Implement adding to address list
            MessageBox.Show($"Adding pointer to address list: {baseAddress} -> {pointerPath} -> {finalAddress}", "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        /// <summary>
        /// Handles the pointer scanner pointer scan progress event
        /// </summary>
        private void PointerScanner_PointerScanProgress(object sender, PointerScanProgressEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => PointerScanner_PointerScanProgress(sender, e)));
                return;
            }
            
            // Update progress bar
            progressBar.Value = e.ProgressPercentage;
            
            // Update status
            statusLabel.Text = $"Scanning... {e.ProgressPercentage}% complete. Found {e.PointersFound} pointers.";
        }
        
        /// <summary>
        /// Handles the pointer scanner pointer scan complete event
        /// </summary>
        private void PointerScanner_PointerScanComplete(object sender, PointerScanCompleteEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => PointerScanner_PointerScanComplete(sender, e)));
                return;
            }
            
            // Update UI
            _isScanning = false;
            scanButton.Text = "Scan";
            progressBar.Value = 100;
            
            // Update status
            statusLabel.Text = $"Scan complete. Found {e.Results.Count} pointers.";
            
            // Update results list
            pointerResultsListView.BeginUpdate();
            pointerResultsListView.Items.Clear();
            
            foreach (var result in e.Results)
            {
                ListViewItem item = new ListViewItem(result.BaseAddress.ToString("X"));
                item.SubItems.Add(FormatPointerPath(result.Offsets));
                item.SubItems.Add(result.FinalAddress.ToString("X"));
                pointerResultsListView.Items.Add(item);
            }
            
            pointerResultsListView.EndUpdate();
        }
        
        /// <summary>
        /// Formats a pointer path as a string
        /// </summary>
        /// <param name="offsets">The offsets</param>
        /// <returns>The formatted pointer path</returns>
        private string FormatPointerPath(List<int> offsets)
        {
            if (offsets == null || offsets.Count == 0)
            {
                return string.Empty;
            }
            
            return string.Join(" -> ", offsets.ConvertAll(offset => $"0x{offset:X}"));
        }
        
        /// <summary>
        /// Tries to parse an address string
        /// </summary>
        /// <param name="addressText">The address text</param>
        /// <param name="address">The parsed address</param>
        /// <returns>True if successful, false otherwise</returns>
        private bool TryParseAddress(string addressText, out IntPtr address)
        {
            address = IntPtr.Zero;
            
            try
            {
                // Remove 0x prefix if present
                if (addressText.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    addressText = addressText.Substring(2);
                }
                
                // Parse as hexadecimal
                long value = Convert.ToInt64(addressText, 16);
                address = new IntPtr(value);
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        #region Designer Generated Code
        
        private System.ComponentModel.IContainer components = null;
        private Label targetAddressLabel;
        private TextBox targetAddressTextBox;
        private Label maxLevelLabel;
        private ComboBox maxLevelComboBox;
        private Button scanButton;
        private ProgressBar progressBar;
        private ListView pointerResultsListView;
        private Button addToAddressListButton;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        
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
            this.targetAddressLabel = new System.Windows.Forms.Label();
            this.targetAddressTextBox = new System.Windows.Forms.TextBox();
            this.maxLevelLabel = new System.Windows.Forms.Label();
            this.maxLevelComboBox = new System.Windows.Forms.ComboBox();
            this.scanButton = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.pointerResultsListView = new System.Windows.Forms.ListView();
            this.addToAddressListButton = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // targetAddressLabel
            // 
            this.targetAddressLabel.AutoSize = true;
            this.targetAddressLabel.Location = new System.Drawing.Point(12, 15);
            this.targetAddressLabel.Name = "targetAddressLabel";
            this.targetAddressLabel.Size = new System.Drawing.Size(85, 15);
            this.targetAddressLabel.TabIndex = 0;
            this.targetAddressLabel.Text = "Target Address:";
            // 
            // targetAddressTextBox
            // 
            this.targetAddressTextBox.Location = new System.Drawing.Point(103, 12);
            this.targetAddressTextBox.Name = "targetAddressTextBox";
            this.targetAddressTextBox.Size = new System.Drawing.Size(120, 23);
            this.targetAddressTextBox.TabIndex = 1;
            // 
            // maxLevelLabel
            // 
            this.maxLevelLabel.AutoSize = true;
            this.maxLevelLabel.Location = new System.Drawing.Point(229, 15);
            this.maxLevelLabel.Name = "maxLevelLabel";
            this.maxLevelLabel.Size = new System.Drawing.Size(63, 15);
            this.maxLevelLabel.TabIndex = 2;
            this.maxLevelLabel.Text = "Max Level:";
            // 
            // maxLevelComboBox
            // 
            this.maxLevelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.maxLevelComboBox.FormattingEnabled = true;
            this.maxLevelComboBox.Location = new System.Drawing.Point(298, 12);
            this.maxLevelComboBox.Name = "maxLevelComboBox";
            this.maxLevelComboBox.Size = new System.Drawing.Size(60, 23);
            this.maxLevelComboBox.TabIndex = 3;
            // 
            // scanButton
            // 
            this.scanButton.Location = new System.Drawing.Point(364, 11);
            this.scanButton.Name = "scanButton";
            this.scanButton.Size = new System.Drawing.Size(75, 23);
            this.scanButton.TabIndex = 4;
            this.scanButton.Text = "Scan";
            this.scanButton.UseVisualStyleBackColor = true;
            this.scanButton.Click += new System.EventHandler(this.scanButton_Click);
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 41);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(560, 23);
            this.progressBar.TabIndex = 5;
            // 
            // pointerResultsListView
            // 
            this.pointerResultsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pointerResultsListView.HideSelection = false;
            this.pointerResultsListView.Location = new System.Drawing.Point(12, 70);
            this.pointerResultsListView.MultiSelect = false;
            this.pointerResultsListView.Name = "pointerResultsListView";
            this.pointerResultsListView.Size = new System.Drawing.Size(560, 300);
            this.pointerResultsListView.TabIndex = 6;
            this.pointerResultsListView.UseCompatibleStateImageBehavior = false;
            // 
            // addToAddressListButton
            // 
            this.addToAddressListButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.addToAddressListButton.Location = new System.Drawing.Point(445, 376);
            this.addToAddressListButton.Name = "addToAddressListButton";
            this.addToAddressListButton.Size = new System.Drawing.Size(127, 23);
            this.addToAddressListButton.TabIndex = 7;
            this.addToAddressListButton.Text = "Add to Address List";
            this.addToAddressListButton.UseVisualStyleBackColor = true;
            this.addToAddressListButton.Click += new System.EventHandler(this.addToAddressListButton_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 411);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(584, 22);
            this.statusStrip.TabIndex = 8;
            this.statusStrip.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(39, 17);
            this.statusLabel.Text = "Ready";
            // 
            // PointerScannerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 433);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.addToAddressListButton);
            this.Controls.Add(this.pointerResultsListView);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.scanButton);
            this.Controls.Add(this.maxLevelComboBox);
            this.Controls.Add(this.maxLevelLabel);
            this.Controls.Add(this.targetAddressTextBox);
            this.Controls.Add(this.targetAddressLabel);
            this.MinimumSize = new System.Drawing.Size(400, 300);
            this.Name = "PointerScannerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Pointer Scanner";
            this.Load += new System.EventHandler(this.PointerScannerForm_Load);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        
        #endregion
    }
}
