using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using CheatEngine.NET.Core;
using CheatEngine.NET.Scripting;
using CheatEngine.NET.Utils;

namespace CheatEngine.NET.GUI
{
    /// <summary>
    /// Form for editing and executing Lua scripts
    /// </summary>
    public partial class LuaScriptForm : Form
    {
        private readonly LuaEngine _luaEngine;
        private string _currentFilePath = string.Empty;
        private bool _isModified = false;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="LuaScriptForm"/> class
        /// </summary>
        public LuaScriptForm()
        {
            InitializeComponent();
            
            // Create Lua engine
            _luaEngine = new LuaEngine();
            
            // Set up event handlers
            _luaEngine.ScriptOutput += LuaEngine_ScriptOutput;
            _luaEngine.ScriptError += LuaEngine_ScriptError;
            _luaEngine.ScriptExecutionCompleted += LuaEngine_ScriptExecutionCompleted;
            
            // Set up script text box
            scriptTextBox.Font = new Font("Consolas", 10);
            scriptTextBox.AcceptsTab = true;
            scriptTextBox.WordWrap = false;
            scriptTextBox.TextChanged += ScriptTextBox_TextChanged;
            
            // Set up output text box
            outputTextBox.Font = new Font("Consolas", 10);
            outputTextBox.ReadOnly = true;
            outputTextBox.BackColor = Color.Black;
            outputTextBox.ForeColor = Color.LightGreen;
            
            // Set up status label
            statusLabel.Text = "Ready";
        }
        
        /// <summary>
        /// Handles the form load event
        /// </summary>
        private void LuaScriptForm_Load(object sender, EventArgs e)
        {
            // Check if a process is attached
            if (CheatEngineCore.TargetProcess == null)
            {
                MessageBox.Show("No process is attached. Some Lua functions may not work correctly.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
            // Initialize Lua engine
            try
            {
                _luaEngine.Initialize();
                AppendOutput("Lua engine initialized successfully.");
            }
            catch (Exception ex)
            {
                AppendOutput($"Error initializing Lua engine: {ex.Message}", true);
            }
        }
        
        /// <summary>
        /// Handles the form closing event
        /// </summary>
        private void LuaScriptForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Check if script is modified
            if (_isModified)
            {
                DialogResult result = MessageBox.Show("The script has been modified. Do you want to save changes?", "Save Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    SaveScript();
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
        
        /// <summary>
        /// Handles the new menu item click event
        /// </summary>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if script is modified
            if (_isModified)
            {
                DialogResult result = MessageBox.Show("The script has been modified. Do you want to save changes?", "Save Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    SaveScript();
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }
            
            // Clear script
            scriptTextBox.Clear();
            _currentFilePath = string.Empty;
            _isModified = false;
            
            // Update title
            UpdateTitle();
        }
        
        /// <summary>
        /// Handles the open menu item click event
        /// </summary>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if script is modified
            if (_isModified)
            {
                DialogResult result = MessageBox.Show("The script has been modified. Do you want to save changes?", "Save Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                
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
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Lua Scripts (*.lua)|*.lua|All Files (*.*)|*.*";
                dialog.Title = "Open Lua Script";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Load script
                        string script = File.ReadAllText(dialog.FileName);
                        scriptTextBox.Text = script;
                        _currentFilePath = dialog.FileName;
                        _isModified = false;
                        
                        // Update title
                        UpdateTitle();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        /// Handles the run menu item click event
        /// </summary>
        private async void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await ExecuteScript();
        }
        
        /// <summary>
        /// Handles the run button click event
        /// </summary>
        private async void runButton_Click(object sender, EventArgs e)
        {
            await ExecuteScript();
        }
        
        /// <summary>
        /// Handles the stop button click event
        /// </summary>
        private void stopButton_Click(object sender, EventArgs e)
        {
            StopScript();
        }
        
        /// <summary>
        /// Handles the clear output button click event
        /// </summary>
        private void clearOutputButton_Click(object sender, EventArgs e)
        {
            outputTextBox.Clear();
        }
        
        /// <summary>
        /// Handles the script text box text changed event
        /// </summary>
        private void ScriptTextBox_TextChanged(object sender, EventArgs e)
        {
            _isModified = true;
            UpdateTitle();
        }
        
        /// <summary>
        /// Handles the Lua engine script output event
        /// </summary>
        private void LuaEngine_ScriptOutput(object sender, LuaOutputEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<LuaOutputEventArgs>(LuaEngine_ScriptOutput), sender, e);
                return;
            }
            
            AppendOutput(e.Text, false);
        }
        
        /// <summary>
        /// Handles the Lua engine script error event
        /// </summary>
        private void LuaEngine_ScriptError(object sender, LuaOutputEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<LuaOutputEventArgs>(LuaEngine_ScriptError), sender, e);
                return;
            }
            
            AppendOutput(e.Text, true);
        }
        
        /// <summary>
        /// Handles the Lua engine script execution completed event
        /// </summary>
        private void LuaEngine_ScriptExecutionCompleted(object sender, LuaScriptEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<LuaScriptEventArgs>(LuaEngine_ScriptExecutionCompleted), sender, e);
                return;
            }
            
            // Update status
            statusLabel.Text = e.Success ? "Script executed successfully." : $"Script execution failed: {e.ErrorMessage}";
            
            // Enable controls
            scriptTextBox.Enabled = true;
            runButton.Enabled = true;
            stopButton.Enabled = false;
            menuStrip.Enabled = true;
        }
        
        /// <summary>
        /// Executes the current script
        /// </summary>
        private async Task ExecuteScript()
        {
            // Check if script is empty
            if (string.IsNullOrWhiteSpace(scriptTextBox.Text))
            {
                MessageBox.Show("Script is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            try
            {
                // Disable controls
                scriptTextBox.Enabled = false;
                runButton.Enabled = false;
                stopButton.Enabled = true;
                menuStrip.Enabled = false;
                
                // Update status
                statusLabel.Text = "Executing script...";
                
                // Clear output
                outputTextBox.Clear();
                
                // Execute script
                await _luaEngine.ExecuteScriptAsync(scriptTextBox.Text);
            }
            catch (Exception ex)
            {
                AppendOutput($"Error executing script: {ex.Message}", true);
                
                // Enable controls
                scriptTextBox.Enabled = true;
                runButton.Enabled = true;
                stopButton.Enabled = false;
                menuStrip.Enabled = true;
                
                // Update status
                statusLabel.Text = "Script execution failed.";
            }
        }
        
        /// <summary>
        /// Stops the current script execution
        /// </summary>
        private void StopScript()
        {
            try
            {
                _luaEngine.StopScript();
                AppendOutput("Script execution stopped.");
                
                // Enable controls
                scriptTextBox.Enabled = true;
                runButton.Enabled = true;
                stopButton.Enabled = false;
                menuStrip.Enabled = true;
                
                // Update status
                statusLabel.Text = "Script execution stopped.";
            }
            catch (Exception ex)
            {
                AppendOutput($"Error stopping script: {ex.Message}", true);
            }
        }
        
        /// <summary>
        /// Saves the current script
        /// </summary>
        private void SaveScript()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                SaveScriptAs();
            }
            else
            {
                try
                {
                    File.WriteAllText(_currentFilePath, scriptTextBox.Text);
                    _isModified = false;
                    UpdateTitle();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        /// <summary>
        /// Saves the current script as a new file
        /// </summary>
        private void SaveScriptAs()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Lua Scripts (*.lua)|*.lua|All Files (*.*)|*.*";
                dialog.Title = "Save Lua Script";
                dialog.DefaultExt = "lua";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(dialog.FileName, scriptTextBox.Text);
                        _currentFilePath = dialog.FileName;
                        _isModified = false;
                        UpdateTitle();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        
        /// <summary>
        /// Updates the form title
        /// </summary>
        private void UpdateTitle()
        {
            string fileName = string.IsNullOrEmpty(_currentFilePath) ? "Untitled" : Path.GetFileName(_currentFilePath);
            Text = $"Lua Script Editor - {fileName}{(_isModified ? "*" : "")}";
        }
        
        /// <summary>
        /// Appends text to the output text box
        /// </summary>
        /// <param name="text">The text to append</param>
        /// <param name="isError">Whether the text is an error message</param>
        private void AppendOutput(string text, bool isError = false)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            
            // Set text color
            outputTextBox.SelectionStart = outputTextBox.TextLength;
            outputTextBox.SelectionLength = 0;
            outputTextBox.SelectionColor = isError ? Color.Red : Color.LightGreen;
            
            // Append text
            outputTextBox.AppendText(text + Environment.NewLine);
            
            // Scroll to end
            outputTextBox.SelectionStart = outputTextBox.TextLength;
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
        private ToolStripMenuItem scriptToolStripMenuItem;
        private ToolStripMenuItem runToolStripMenuItem;
        private SplitContainer splitContainer;
        private TextBox scriptTextBox;
        private RichTextBox outputTextBox;
        private ToolStrip toolStrip;
        private ToolStripButton runButton;
        private ToolStripButton stopButton;
        private ToolStripButton clearOutputButton;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LuaScriptForm));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.scriptTextBox = new System.Windows.Forms.TextBox();
            this.outputTextBox = new System.Windows.Forms.RichTextBox();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.runButton = new System.Windows.Forms.ToolStripButton();
            this.stopButton = new System.Windows.Forms.ToolStripButton();
            this.clearOutputButton = new System.Windows.Forms.ToolStripButton();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.scriptToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(784, 24);
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
            // scriptToolStripMenuItem
            // 
            this.scriptToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runToolStripMenuItem});
            this.scriptToolStripMenuItem.Name = "scriptToolStripMenuItem";
            this.scriptToolStripMenuItem.Size = new System.Drawing.Size(49, 20);
            this.scriptToolStripMenuItem.Text = "&Script";
            // 
            // runToolStripMenuItem
            // 
            this.runToolStripMenuItem.Name = "runToolStripMenuItem";
            this.runToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.runToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.runToolStripMenuItem.Text = "&Run";
            this.runToolStripMenuItem.Click += new System.EventHandler(this.runToolStripMenuItem_Click);
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(0, 52);
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
            this.splitContainer.Size = new System.Drawing.Size(784, 487);
            this.splitContainer.SplitterDistance = 300;
            this.splitContainer.TabIndex = 1;
            // 
            // scriptTextBox
            // 
            this.scriptTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scriptTextBox.Location = new System.Drawing.Point(0, 0);
            this.scriptTextBox.Multiline = true;
            this.scriptTextBox.Name = "scriptTextBox";
            this.scriptTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.scriptTextBox.Size = new System.Drawing.Size(784, 300);
            this.scriptTextBox.TabIndex = 0;
            // 
            // outputTextBox
            // 
            this.outputTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputTextBox.Location = new System.Drawing.Point(0, 0);
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.Size = new System.Drawing.Size(784, 183);
            this.outputTextBox.TabIndex = 0;
            this.outputTextBox.Text = "";
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runButton,
            this.stopButton,
            this.clearOutputButton});
            this.toolStrip.Location = new System.Drawing.Point(0, 24);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(784, 25);
            this.toolStrip.TabIndex = 2;
            this.toolStrip.Text = "toolStrip1";
            // 
            // runButton
            // 
            this.runButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.runButton.Image = ((System.Drawing.Image)(resources.GetObject("runButton.Image")));
            this.runButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(23, 22);
            this.runButton.Text = "Run";
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.stopButton.Enabled = false;
            this.stopButton.Image = ((System.Drawing.Image)(resources.GetObject("stopButton.Image")));
            this.stopButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(23, 22);
            this.stopButton.Text = "Stop";
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // clearOutputButton
            // 
            this.clearOutputButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.clearOutputButton.Image = ((System.Drawing.Image)(resources.GetObject("clearOutputButton.Image")));
            this.clearOutputButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.clearOutputButton.Name = "clearOutputButton";
            this.clearOutputButton.Size = new System.Drawing.Size(23, 22);
            this.clearOutputButton.Text = "Clear Output";
            this.clearOutputButton.Click += new System.EventHandler(this.clearOutputButton_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 539);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(784, 22);
            this.statusStrip.TabIndex = 3;
            this.statusStrip.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(39, 17);
            this.statusLabel.Text = "Ready";
            // 
            // LuaScriptForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.MinimumSize = new System.Drawing.Size(400, 300);
            this.Name = "LuaScriptForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Lua Script Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LuaScriptForm_FormClosing);
            this.Load += new System.EventHandler(this.LuaScriptForm_Load);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        
        #endregion
    }
}
