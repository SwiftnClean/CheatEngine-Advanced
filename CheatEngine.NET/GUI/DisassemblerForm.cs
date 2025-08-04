using System;
using System.Drawing;
using System.Windows.Forms;
using CheatEngine.NET.Core;
using CheatEngine.NET.Disassembler;
using CheatEngine.NET.Utils;

namespace CheatEngine.NET.GUI
{
    /// <summary>
    /// Form for viewing disassembled code
    /// </summary>
    public partial class DisassemblerForm : Form
    {
        private readonly Disassembler.Disassembler _disassembler;
        private int _selectedIndex = -1;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DisassemblerForm"/> class
        /// </summary>
        public DisassemblerForm()
        {
            InitializeComponent();
            
            // Create disassembler
            _disassembler = new Disassembler.Disassembler();
            
            // Set up event handlers
            _disassembler.DisassemblyRefreshed += Disassembler_DisassemblyRefreshed;
            
            // Set up address text box
            addressTextBox.Text = "0";
        }
        
        /// <summary>
        /// Handles the form load event
        /// </summary>
        private void DisassemblerForm_Load(object sender, EventArgs e)
        {
            // Check if a process is attached
            if (CheatEngineCore.TargetProcess == null)
            {
                MessageBox.Show("No process is attached. Please attach to a process first.", "No Process", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }
            
            // Set up disassembler
            _disassembler.InstructionCount = 20;
            _disassembler.ShowBytes = true;
            _disassembler.ShowAddresses = true;
            
            // Navigate to base address
            NavigateToAddress(new IntPtr(0x400000)); // Default base address for most Windows executables
        }
        
        /// <summary>
        /// Handles the disassembly list box draw item event
        /// </summary>
        private void disassemblyListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Check if valid index
            if (e.Index < 0 || e.Index >= _disassembler.Instructions.Count)
            {
                return;
            }
            
            // Get instruction
            DisassembledInstruction instruction = _disassembler.Instructions[e.Index];
            
            // Set background color
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
            }
            
            // Set text color
            Brush textBrush = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                ? SystemBrushes.HighlightText
                : SystemBrushes.WindowText;
            
            // Draw instruction
            string text = _disassembler.GetFormattedInstruction(instruction);
            e.Graphics.DrawString(text, e.Font, textBrush, e.Bounds.Left, e.Bounds.Top);
        }
        
        /// <summary>
        /// Handles the disassembly list box selected index changed event
        /// </summary>
        private void disassemblyListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedIndex = disassemblyListBox.SelectedIndex;
            
            // Update selected instruction info
            if (_selectedIndex >= 0 && _selectedIndex < _disassembler.Instructions.Count)
            {
                DisassembledInstruction instruction = _disassembler.Instructions[_selectedIndex];
                
                // Update address text box
                addressTextBox.Text = $"{instruction.Address.ToInt64():X}";
                
                // Update status label
                statusLabel.Text = $"Selected: {instruction.AddressString} - {instruction.Mnemonic} {instruction.Operands}";
            }
        }
        
        /// <summary>
        /// Handles the disassembly list box key down event
        /// </summary>
        private void disassemblyListBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle navigation keys
            switch (e.KeyCode)
            {
                case Keys.PageUp:
                    NavigatePrevious();
                    break;
                
                case Keys.PageDown:
                    NavigateNext();
                    break;
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
            _disassembler.RefreshDisassembly();
        }
        
        /// <summary>
        /// Handles the follow button click event
        /// </summary>
        private void followButton_Click(object sender, EventArgs e)
        {
            // Check if an instruction is selected
            if (_selectedIndex >= 0 && _selectedIndex < _disassembler.Instructions.Count)
            {
                DisassembledInstruction instruction = _disassembler.Instructions[_selectedIndex];
                
                // Check if instruction is a jump or call
                if (instruction.Mnemonic.StartsWith("J") || instruction.Mnemonic == "CALL")
                {
                    // Parse target address
                    if (TryParseAddress(instruction.Operands, out IntPtr address))
                    {
                        NavigateToAddress(address);
                    }
                }
            }
        }
        
        /// <summary>
        /// Handles the disassembler disassembly refreshed event
        /// </summary>
        private void Disassembler_DisassemblyRefreshed(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Disassembler_DisassemblyRefreshed(sender, e)));
                return;
            }
            
            // Update address text box
            addressTextBox.Text = $"{_disassembler.BaseAddress.ToInt64():X}";
            
            // Update list box
            disassemblyListBox.BeginUpdate();
            disassemblyListBox.Items.Clear();
            
            foreach (var instruction in _disassembler.Instructions)
            {
                disassemblyListBox.Items.Add(instruction);
            }
            
            disassemblyListBox.EndUpdate();
            
            // Select first item
            if (disassemblyListBox.Items.Count > 0)
            {
                disassemblyListBox.SelectedIndex = 0;
            }
        }
        
        /// <summary>
        /// Navigates to a specific address
        /// </summary>
        /// <param name="address">The address to navigate to</param>
        private void NavigateToAddress(IntPtr address)
        {
            try
            {
                _disassembler.NavigateTo(address);
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
                _disassembler.NavigatePrevious();
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
                _disassembler.NavigateNext();
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
        
        #region Designer Generated Code
        
        private System.ComponentModel.IContainer components = null;
        private ListBox disassemblyListBox;
        private TextBox addressTextBox;
        private Button goButton;
        private Button previousButton;
        private Button nextButton;
        private Button refreshButton;
        private Button followButton;
        private Label addressLabel;
        private Label statusLabel;
        
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
            this.disassemblyListBox = new System.Windows.Forms.ListBox();
            this.addressTextBox = new System.Windows.Forms.TextBox();
            this.goButton = new System.Windows.Forms.Button();
            this.previousButton = new System.Windows.Forms.Button();
            this.nextButton = new System.Windows.Forms.Button();
            this.refreshButton = new System.Windows.Forms.Button();
            this.followButton = new System.Windows.Forms.Button();
            this.addressLabel = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // disassemblyListBox
            // 
            this.disassemblyListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.disassemblyListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.disassemblyListBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.disassemblyListBox.FormattingEnabled = true;
            this.disassemblyListBox.ItemHeight = 14;
            this.disassemblyListBox.Location = new System.Drawing.Point(12, 41);
            this.disassemblyListBox.Name = "disassemblyListBox";
            this.disassemblyListBox.Size = new System.Drawing.Size(560, 326);
            this.disassemblyListBox.TabIndex = 0;
            this.disassemblyListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.disassemblyListBox_DrawItem);
            this.disassemblyListBox.SelectedIndexChanged += new System.EventHandler(this.disassemblyListBox_SelectedIndexChanged);
            this.disassemblyListBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.disassemblyListBox_KeyDown);
            // 
            // addressTextBox
            // 
            this.addressTextBox.Location = new System.Drawing.Point(65, 12);
            this.addressTextBox.Name = "addressTextBox";
            this.addressTextBox.Size = new System.Drawing.Size(120, 23);
            this.addressTextBox.TabIndex = 1;
            // 
            // goButton
            // 
            this.goButton.Location = new System.Drawing.Point(191, 11);
            this.goButton.Name = "goButton";
            this.goButton.Size = new System.Drawing.Size(50, 23);
            this.goButton.TabIndex = 2;
            this.goButton.Text = "Go";
            this.goButton.UseVisualStyleBackColor = true;
            this.goButton.Click += new System.EventHandler(this.goButton_Click);
            // 
            // previousButton
            // 
            this.previousButton.Location = new System.Drawing.Point(247, 11);
            this.previousButton.Name = "previousButton";
            this.previousButton.Size = new System.Drawing.Size(75, 23);
            this.previousButton.TabIndex = 3;
            this.previousButton.Text = "Previous";
            this.previousButton.UseVisualStyleBackColor = true;
            this.previousButton.Click += new System.EventHandler(this.previousButton_Click);
            // 
            // nextButton
            // 
            this.nextButton.Location = new System.Drawing.Point(328, 11);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(75, 23);
            this.nextButton.TabIndex = 4;
            this.nextButton.Text = "Next";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // refreshButton
            // 
            this.refreshButton.Location = new System.Drawing.Point(409, 11);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(75, 23);
            this.refreshButton.TabIndex = 5;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // followButton
            // 
            this.followButton.Location = new System.Drawing.Point(490, 11);
            this.followButton.Name = "followButton";
            this.followButton.Size = new System.Drawing.Size(75, 23);
            this.followButton.TabIndex = 6;
            this.followButton.Text = "Follow";
            this.followButton.UseVisualStyleBackColor = true;
            this.followButton.Click += new System.EventHandler(this.followButton_Click);
            // 
            // addressLabel
            // 
            this.addressLabel.AutoSize = true;
            this.addressLabel.Location = new System.Drawing.Point(12, 15);
            this.addressLabel.Name = "addressLabel";
            this.addressLabel.Size = new System.Drawing.Size(52, 15);
            this.addressLabel.TabIndex = 7;
            this.addressLabel.Text = "Address:";
            // 
            // statusLabel
            // 
            this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusLabel.Location = new System.Drawing.Point(12, 376);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(560, 23);
            this.statusLabel.TabIndex = 8;
            this.statusLabel.Text = "Ready";
            // 
            // DisassemblerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 411);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.addressLabel);
            this.Controls.Add(this.followButton);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.nextButton);
            this.Controls.Add(this.previousButton);
            this.Controls.Add(this.goButton);
            this.Controls.Add(this.addressTextBox);
            this.Controls.Add(this.disassemblyListBox);
            this.MinimumSize = new System.Drawing.Size(400, 300);
            this.Name = "DisassemblerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Disassembler";
            this.Load += new System.EventHandler(this.DisassemblerForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        
        #endregion
    }
}
