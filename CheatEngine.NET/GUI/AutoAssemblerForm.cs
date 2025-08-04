using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using CheatEngine.NET.Core;
using CheatEngine.NET.Memory;
using CheatEngine.NET.Utils;

namespace CheatEngine.NET.GUI
{
    /// <summary>
    /// Form for writing and executing assembly code in the target process
    /// </summary>
    public partial class AutoAssemblerForm : Form
    {
        private readonly AutoAssembler _autoAssembler;
        private bool _isAssembling = false;
        private string _currentFilePath = null;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoAssemblerForm"/> class
        /// </summary>
        public AutoAssemblerForm()
        {
            InitializeComponent();
            
            // Create auto assembler
            _autoAssembler = new AutoAssembler();
            
            // Set up event handlers
            _autoAssembler.AssemblyProgress += AutoAssembler_AssemblyProgress;
            _autoAssembler.AssemblyComplete += AutoAssembler_AssemblyComplete;
        }
        
        /// <summary>
        /// Handles the form load event
        /// </summary>
        private void AutoAssemblerForm_Load(object sender, EventArgs e)
        {
            // Check if a process is attached
            if (CheatEngineCore.TargetProcess == null)
            {
                MessageBox.Show("No process is attached. Please attach to a process first.", "No Process", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }
            
            // Set up script editor
            scriptTextBox.AcceptsTab = true;
            scriptTextBox.Font = new Font("Consolas", 10F);
            
            // Set up output text box
            outputTextBox.ReadOnly = true;
            outputTextBox.Font = new Font("Consolas", 10F);
            
            // Update status
            UpdateStatus("Ready");
        }
        
        /// <summary>
        /// Handles the new menu item click event
        /// </summary>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if there are unsaved changes
            if (scriptTextBox.Modified)
            {
                DialogResult result = MessageBox.Show(
                    "Do you want to save changes to the current script?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    SaveScript();
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }
            
            // Clear script editor
            scriptTextBox.Clear();
            scriptTextBox.Modified = false;
            _currentFilePath = null;
            
            // Update form title
            UpdateFormTitle();
        }
        
        /// <summary>
        /// Handles the open menu item click event
        /// </summary>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if there are unsaved changes
            if (scriptTextBox.Modified)
            {
                DialogResult result = MessageBox.Show(
                    "Do you want to save changes to the current script?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    SaveScript();
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }
            
            // Show open file dialog
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Assembly Scripts (*.asm)|*.asm|All Files (*.*)|*.*";
                openFileDialog.Title = "Open Assembly Script";
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Load script from file
                        scriptTextBox.Text = File.ReadAllText(openFileDialog.FileName);
                        scriptTextBox.Modified = false;
                        _currentFilePath = openFileDialog.FileName;
                        
                        // Update form title
                        UpdateFormTitle();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Error opening file: {ex.Message}",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }
        
        /// <summary>
        /// Handles the save menu item click event
        /// </summary>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveScript();
        }
        
        /// <summary>
        /// Handles the save as menu item click event
        /// </summary>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveScriptAs();
        }
        
        /// <summary>
        /// Handles the exit menu item click event
        /// </summary>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        
        /// <summary>
        /// Handles the assemble button click event
        /// </summary>
        private async void assembleButton_Click(object sender, EventArgs e)
        {
            if (_isAssembling)
            {
                // Stop assembling
                _autoAssembler.StopAssembly();
                return;
            }
            
            // Get script
            string script = scriptTextBox.Text;
            
            // Check if script is empty
            if (string.IsNullOrWhiteSpace(script))
            {
                MessageBox.Show("Script is empty. Please enter some assembly code.", "Empty Script", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Start assembling
            _isAssembling = true;
            assembleButton.Text = "Stop";
            outputTextBox.Clear();
            progressBar.Value = 0;
            
            try
            {
                await _autoAssembler.AssembleAndExecuteAsync(script);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error assembling script: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isAssembling = false;
                assembleButton.Text = "Assemble";
                UpdateStatus("Assembly failed.");
            }
        }
        
        /// <summary>
        /// Handles the auto assembler assembly progress event
        /// </summary>
        private void AutoAssembler_AssemblyProgress(object sender, AssemblyProgressEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AutoAssembler_AssemblyProgress(sender, e)));
                return;
            }
            
            // Update progress bar
            progressBar.Value = e.ProgressPercentage;
            
            // Update output
            AppendOutput(e.Message);
            
            // Update status
            UpdateStatus($"Assembling... {e.ProgressPercentage}% complete.");
        }
        
        /// <summary>
        /// Handles the auto assembler assembly complete event
        /// </summary>
        private void AutoAssembler_AssemblyComplete(object sender, AssemblyCompleteEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AutoAssembler_AssemblyComplete(sender, e)));
                return;
            }
            
            // Update UI
            _isAssembling = false;
            assembleButton.Text = "Assemble";
            progressBar.Value = 100;
            
            // Update output
            AppendOutput(e.Success ? "Assembly completed successfully." : "Assembly failed.");
            
            // Update status
            UpdateStatus(e.Success ? "Assembly completed successfully." : "Assembly failed.");
        }
        
        /// <summary>
        /// Saves the current script
        /// </summary>
        private void SaveScript()
        {
            if (_currentFilePath == null)
            {
                SaveScriptAs();
            }
            else
            {
                try
                {
                    // Save script to file
                    File.WriteAllText(_currentFilePath, scriptTextBox.Text);
                    scriptTextBox.Modified = false;
                    
                    // Update status
                    UpdateStatus($"Saved to {_currentFilePath}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error saving file: {ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
        
        /// <summary>
        /// Saves the current script as a new file
        /// </summary>
        private void SaveScriptAs()
        {
            // Show save file dialog
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Assembly Scripts (*.asm)|*.asm|All Files (*.*)|*.*";
                saveFileDialog.Title = "Save Assembly Script";
                
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Save script to file
                        File.WriteAllText(saveFileDialog.FileName, scriptTextBox.Text);
                        scriptTextBox.Modified = false;
                        _currentFilePath = saveFileDialog.FileName;
                        
                        // Update form title
                        UpdateFormTitle();
                        
                        // Update status
                        UpdateStatus($"Saved to {_currentFilePath}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Error saving file: {ex.Message}",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }
        
        /// <summary>
        /// Updates the form title
        /// </summary>
        private void UpdateFormTitle()
        {
            if (_currentFilePath == null)
            {
                Text = "Auto Assembler - New Script";
            }
            else
            {
                Text = $"Auto Assembler - {Path.GetFileName(_currentFilePath)}";
            }
        }
        
        /// <summary>
        /// Updates the status label
        /// </summary>
        /// <param name="status">The status text</param>
        private void UpdateStatus(string status)
        {
            statusLabel.Text = status;
        }
        
        /// <summary>
        /// Appends text to the output text box
        /// </summary>
        /// <param name="text">The text to append</param>
        private void AppendOutput(string text)
        {
            outputTextBox.AppendText(text + Environment.NewLine);
            outputTextBox.ScrollToCaret();
        }
        
        #region Designer Generated Code
        
        private System.ComponentModel.IContainer components = null;
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem newToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem exitToolStripMenuItem;
        private SplitContainer splitContainer;
        private TextBox scriptTextBox;
        private TextBox outputTextBox;
        private Button assembleButton;
        private ProgressBar progressBar;
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
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.scriptTextBox = new System.Windows.Forms.TextBox();
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.assembleButton = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(684, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.newToolStripMenuItem.Text = "&New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.openToolStripMenuItem.Text = "&Open...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.saveAsToolStripMenuItem.Text = "Save &As...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(183, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(12, 27);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.scriptTextBox);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.outputTextBox);
            this.splitContainer.Size = new System.Drawing.Size(660, 400);
            this.splitContainer.SplitterDistance = 250;
            this.splitContainer.TabIndex = 1;
            // 
            // scriptTextBox
            // 
            this.scriptTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scriptTextBox.Location = new System.Drawing.Point(0, 0);
            this.scriptTextBox.Multiline = true;
            this.scriptTextBox.Name = "scriptTextBox";
            this.scriptTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.scriptTextBox.Size = new System.Drawing.Size(660, 250);
            this.scriptTextBox.TabIndex = 0;
            this.scriptTextBox.WordWrap = false;
            // 
            // outputTextBox
            // 
            this.outputTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputTextBox.Location = new System.Drawing.Point(0, 0);
            this.outputTextBox.Multiline = true;
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.outputTextBox.Size = new System.Drawing.Size(660, 146);
            this.outputTextBox.TabIndex = 0;
            this.outputTextBox.WordWrap = false;
            // 
            // assembleButton
            // 
            this.assembleButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.assembleButton.Location = new System.Drawing.Point(597, 433);
            this.assembleButton.Name = "assembleButton";
            this.assembleButton.Size = new System.Drawing.Size(75, 23);
            this.assembleButton.TabIndex = 2;
            this.assembleButton.Text = "Assemble";
            this.assembleButton.UseVisualStyleBackColor = true;
            this.assembleButton.Click += new System.EventHandler(this.assembleButton_Click);
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 433);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(579, 23);
            this.progressBar.TabIndex = 3;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 459);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(684, 22);
            this.statusStrip.TabIndex = 4;
            this.statusStrip.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(39, 17);
            this.statusLabel.Text = "Ready";
            // 
            // AutoAssemblerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 481);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.assembleButton);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.MinimumSize = new System.Drawing.Size(400, 300);
            this.Name = "AutoAssemblerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Auto Assembler";
            this.Load += new System.EventHandler(this.AutoAssemblerForm_Load);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        
        #endregion
    }
}
