using System;
using System.Drawing;
using System.Windows.Forms;
using CheatEngine.NET.Core;
using CheatEngine.NET.Memory;
using CheatEngine.NET.Utils;

namespace CheatEngine.NET.GUI
{
    /// <summary>
    /// Form for viewing and editing memory in a hexadecimal format
    /// </summary>
    public partial class MemoryViewerForm : Form
    {
        private readonly MemoryViewer _memoryViewer;
        private readonly MemoryViewerRenderer _renderer;
        private bool _isEditing = false;
        private int _editRow = -1;
        private int _editColumn = -1;
        private string _editValue = string.Empty;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryViewerForm"/> class
        /// </summary>
        public MemoryViewerForm()
        {
            InitializeComponent();
            
            // Create memory viewer
            _memoryViewer = new MemoryViewer();
            _renderer = new MemoryViewerRenderer(_memoryViewer);
            
            // Set up event handlers
            _memoryViewer.MemoryRefreshed += MemoryViewer_MemoryRefreshed;
            
            // Set up address text box
            addressTextBox.Text = "0";
        }
        
        /// <summary>
        /// Handles the form load event
        /// </summary>
        private void MemoryViewerForm_Load(object sender, EventArgs e)
        {
            // Check if a process is attached
            if (CheatEngineCore.TargetProcess == null)
            {
                MessageBox.Show("No process is attached. Please attach to a process first.", "No Process", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }
            
            // Set up memory viewer
            _memoryViewer.BytesPerRow = 16;
            _memoryViewer.TotalRows = 16;
            _memoryViewer.ShowText = true;
            _memoryViewer.ShowAddresses = true;
            
            // Navigate to base address
            NavigateToAddress(new IntPtr(0x400000)); // Default base address for most Windows executables
        }
        
        /// <summary>
        /// Handles the memory viewer panel paint event
        /// </summary>
        private void memoryViewerPanel_Paint(object sender, PaintEventArgs e)
        {
            // Render memory viewer
            _renderer.Render(e.Graphics, memoryViewerPanel.ClientRectangle);
        }
        
        /// <summary>
        /// Handles the memory viewer panel mouse down event
        /// </summary>
        private void memoryViewerPanel_MouseDown(object sender, MouseEventArgs e)
        {
            // Get cell at mouse position
            if (_renderer.GetCellAt(e.Location, memoryViewerPanel.CreateGraphics(), out int row, out int column, out bool isText))
            {
                // Select cell
                _renderer.SelectedRow = row;
                _renderer.SelectedColumn = column;
                
                // Start editing if double-clicked
                if (e.Clicks == 2 && !isText)
                {
                    StartEditing(row, column);
                }
                
                // Refresh panel
                memoryViewerPanel.Invalidate();
            }
        }
        
        /// <summary>
        /// Handles the memory viewer panel key down event
        /// </summary>
        private void memoryViewerPanel_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle navigation keys
            switch (e.KeyCode)
            {
                case Keys.Up:
                    if (_renderer.SelectedRow > 0)
                    {
                        _renderer.SelectedRow--;
                        memoryViewerPanel.Invalidate();
                    }
                    break;
                
                case Keys.Down:
                    if (_renderer.SelectedRow < _memoryViewer.TotalRows - 1)
                    {
                        _renderer.SelectedRow++;
                        memoryViewerPanel.Invalidate();
                    }
                    break;
                
                case Keys.Left:
                    if (_renderer.SelectedColumn > 0)
                    {
                        _renderer.SelectedColumn--;
                        memoryViewerPanel.Invalidate();
                    }
                    break;
                
                case Keys.Right:
                    if (_renderer.SelectedColumn < _memoryViewer.BytesPerRow - 1)
                    {
                        _renderer.SelectedColumn++;
                        memoryViewerPanel.Invalidate();
                    }
                    break;
                
                case Keys.PageUp:
                    NavigatePrevious();
                    break;
                
                case Keys.PageDown:
                    NavigateNext();
                    break;
                
                case Keys.Enter:
                    if (!_isEditing && _renderer.SelectedRow >= 0 && _renderer.SelectedColumn >= 0)
                    {
                        StartEditing(_renderer.SelectedRow, _renderer.SelectedColumn);
                    }
                    else if (_isEditing)
                    {
                        FinishEditing(true);
                    }
                    break;
                
                case Keys.Escape:
                    if (_isEditing)
                    {
                        FinishEditing(false);
                    }
                    break;
            }
            
            // Handle editing keys
            if (_isEditing)
            {
                // Check if key is a hex digit
                if ((e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) || (e.KeyCode >= Keys.A && e.KeyCode <= Keys.F))
                {
                    char c = e.KeyCode.ToString().Last();
                    
                    if (_editValue.Length < 2)
                    {
                        _editValue += c;
                    }
                    
                    if (_editValue.Length == 2)
                    {
                        FinishEditing(true);
                    }
                    
                    memoryViewerPanel.Invalidate();
                }
                else if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
                {
                    if (_editValue.Length > 0)
                    {
                        _editValue = _editValue.Substring(0, _editValue.Length - 1);
                        memoryViewerPanel.Invalidate();
                    }
                }
            }
        }
        
        /// <summary>
        /// Handles the go button click event
        /// </summary>
        private void goButton_Click(object sender, EventArgs e)
        {
            // Parse address
            if (TryParseAddress(addressTextBox.Text, out IntPtr address))
            {
                NavigateToAddress(address);
            }
            else
            {
                MessageBox.Show("Invalid address format. Please enter a valid hexadecimal address.", "Invalid Address", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        /// <summary>
        /// Handles the previous button click event
        /// </summary>
        private void previousButton_Click(object sender, EventArgs e)
        {
            NavigatePrevious();
        }
        
        /// <summary>
        /// Handles the next button click event
        /// </summary>
        private void nextButton_Click(object sender, EventArgs e)
        {
            NavigateNext();
        }
        
        /// <summary>
        /// Handles the refresh button click event
        /// </summary>
        private void refreshButton_Click(object sender, EventArgs e)
        {
            _memoryViewer.RefreshMemory();
        }
        
        /// <summary>
        /// Handles the memory viewer memory refreshed event
        /// </summary>
        private void MemoryViewer_MemoryRefreshed(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => MemoryViewer_MemoryRefreshed(sender, e)));
                return;
            }
            
            // Update address text box
            addressTextBox.Text = $"{_memoryViewer.BaseAddress.ToInt64():X}";
            
            // Refresh panel
            memoryViewerPanel.Invalidate();
        }
        
        /// <summary>
        /// Navigates to a specific address
        /// </summary>
        /// <param name="address">The address to navigate to</param>
        private void NavigateToAddress(IntPtr address)
        {
            try
            {
                _memoryViewer.NavigateTo(address);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to address: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Navigates to the previous page
        /// </summary>
        private void NavigatePrevious()
        {
            try
            {
                _memoryViewer.NavigatePrevious();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to previous page: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Navigates to the next page
        /// </summary>
        private void NavigateNext()
        {
            try
            {
                _memoryViewer.NavigateNext();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to next page: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
        
        /// <summary>
        /// Starts editing a cell
        /// </summary>
        /// <param name="row">The row index</param>
        /// <param name="column">The column index</param>
        private void StartEditing(int row, int column)
        {
            _isEditing = true;
            _editRow = row;
            _editColumn = column;
            _editValue = string.Empty;
            
            // Set focus to panel
            memoryViewerPanel.Focus();
        }
        
        /// <summary>
        /// Finishes editing a cell
        /// </summary>
        /// <param name="apply">Whether to apply the changes</param>
        private void FinishEditing(bool apply)
        {
            if (!_isEditing)
            {
                return;
            }
            
            if (apply && !string.IsNullOrEmpty(_editValue))
            {
                try
                {
                    // Parse value
                    byte value = Convert.ToByte(_editValue, 16);
                    
                    // Write to memory
                    bool success = _memoryViewer.WriteByte(_editRow, _editColumn, value);
                    
                    if (success)
                    {
                        // Mark as modified
                        IntPtr address = _memoryViewer.GetAddressForCell(_editRow, _editColumn);
                        _renderer.MarkModified(address, value);
                    }
                    else
                    {
                        MessageBox.Show("Failed to write value to memory.", "Write Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error writing value: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
            _isEditing = false;
            _editRow = -1;
            _editColumn = -1;
            _editValue = string.Empty;
            
            // Refresh panel
            memoryViewerPanel.Invalidate();
        }
        
        #region Designer Generated Code
        
        private System.ComponentModel.IContainer components = null;
        private Panel memoryViewerPanel;
        private TextBox addressTextBox;
        private Button goButton;
        private Button previousButton;
        private Button nextButton;
        private Button refreshButton;
        private Label addressLabel;
        
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
            this.memoryViewerPanel = new System.Windows.Forms.Panel();
            this.addressTextBox = new System.Windows.Forms.TextBox();
            this.goButton = new System.Windows.Forms.Button();
            this.previousButton = new System.Windows.Forms.Button();
            this.nextButton = new System.Windows.Forms.Button();
            this.refreshButton = new System.Windows.Forms.Button();
            this.addressLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // memoryViewerPanel
            // 
            this.memoryViewerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.memoryViewerPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.memoryViewerPanel.Location = new System.Drawing.Point(12, 41);
            this.memoryViewerPanel.Name = "memoryViewerPanel";
            this.memoryViewerPanel.Size = new System.Drawing.Size(560, 358);
            this.memoryViewerPanel.TabIndex = 0;
            this.memoryViewerPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.memoryViewerPanel_Paint);
            this.memoryViewerPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.memoryViewerPanel_MouseDown);
            this.memoryViewerPanel.KeyDown += new System.Windows.Forms.KeyEventHandler(this.memoryViewerPanel_KeyDown);
            // 
            // addressTextBox
            // 
            this.addressTextBox.Location = new System.Drawing.Point(65, 12);
            this.addressTextBox.Name = "addressTextBox";
            this.addressTextBox.Size = new System.Drawing.Size(120, 20);
            this.addressTextBox.TabIndex = 1;
            // 
            // goButton
            // 
            this.goButton.Location = new System.Drawing.Point(191, 10);
            this.goButton.Name = "goButton";
            this.goButton.Size = new System.Drawing.Size(50, 23);
            this.goButton.TabIndex = 2;
            this.goButton.Text = "Go";
            this.goButton.UseVisualStyleBackColor = true;
            this.goButton.Click += new System.EventHandler(this.goButton_Click);
            // 
            // previousButton
            // 
            this.previousButton.Location = new System.Drawing.Point(247, 10);
            this.previousButton.Name = "previousButton";
            this.previousButton.Size = new System.Drawing.Size(75, 23);
            this.previousButton.TabIndex = 3;
            this.previousButton.Text = "Previous";
            this.previousButton.UseVisualStyleBackColor = true;
            this.previousButton.Click += new System.EventHandler(this.previousButton_Click);
            // 
            // nextButton
            // 
            this.nextButton.Location = new System.Drawing.Point(328, 10);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(75, 23);
            this.nextButton.TabIndex = 4;
            this.nextButton.Text = "Next";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // refreshButton
            // 
            this.refreshButton.Location = new System.Drawing.Point(409, 10);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(75, 23);
            this.refreshButton.TabIndex = 5;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // addressLabel
            // 
            this.addressLabel.AutoSize = true;
            this.addressLabel.Location = new System.Drawing.Point(12, 15);
            this.addressLabel.Name = "addressLabel";
            this.addressLabel.Size = new System.Drawing.Size(48, 13);
            this.addressLabel.TabIndex = 6;
            this.addressLabel.Text = "Address:";
            // 
            // MemoryViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 411);
            this.Controls.Add(this.addressLabel);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.nextButton);
            this.Controls.Add(this.previousButton);
            this.Controls.Add(this.goButton);
            this.Controls.Add(this.addressTextBox);
            this.Controls.Add(this.memoryViewerPanel);
            this.MinimumSize = new System.Drawing.Size(400, 300);
            this.Name = "MemoryViewerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Memory Viewer";
            this.Load += new System.EventHandler(this.MemoryViewerForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        
        #endregion
    }
}
