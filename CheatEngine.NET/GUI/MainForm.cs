using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CheatEngine.NET.Core;
using CheatEngine.NET.Debugger;
using CheatEngine.NET.Memory;
using CheatEngine.NET.Scanner;
using CheatEngine.NET.Disassembler;
using CheatEngine.NET.Scripting;
using CheatEngine.NET.Utils;

namespace CheatEngine.NET.GUI
{
    public partial class MainForm : Form
    {
        private ProcessListManager processListManager;
        private MemoryScanner memoryScanner;
        private AddressListManager addressListManager;
        private DebuggerManager debuggerManager;
        private PointerScanner pointerScanner;
        private LuaEngine luaEngine;
        private bool isScanning = false;

        public MainForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            // Initialize process list manager
            processListManager = new ProcessListManager();
            
            // Initialize memory scanner
            memoryScanner = new MemoryScanner();
            
            // Initialize address list manager
            addressListManager = new AddressListManager();
            
            // Initialize debugger manager
            debuggerManager = new DebuggerManager();
            
            // Initialize pointer scanner
            pointerScanner = new PointerScanner();
            
            // Initialize Lua engine
            luaEngine = new LuaEngine();
            luaEngine.Initialize();
            
            // Set up event handlers
            memoryScanner.ScanComplete += MemoryScanner_ScanComplete;
            memoryScanner.ScanProgress += MemoryScanner_ScanProgress;
            
            // Set up menu items
            SetupMenuItems();
        }

        private void ProcessSelected(int processId, string processName)
        {
            // Update UI with selected process info
            lblSelectedProcess.Text = $"Process: {processName} (PID: {processId})";
            
            // Attach to the selected process
            if (CheatEngineCore.AttachToProcess(processId))
            {
                // Enable scanning controls
                EnableScanningControls(true);
                
                // Update status
                UpdateStatus($"Attached to process {processName} (PID: {processId})");
            }
            else
            {
                MessageBox.Show(
                    "Failed to attach to the selected process. Make sure you have sufficient privileges.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void MemoryScanner_ScanComplete(object sender, ScanCompleteEventArgs e)
        {
            // Update UI with scan results
            isScanning = false;
            btnScan.Text = "New Scan";
            btnNextScan.Enabled = e.ResultCount > 0;
            
            // Update status
            UpdateStatus($"Scan complete. Found {e.ResultCount} results.");
            
            // Update progress bar
            progressBarScan.Value = 100;
            
            // Update address list with results
            addressListManager.UpdateAddressList(e.Results);
        }

        private void MemoryScanner_ScanProgress(object sender, ScanProgressEventArgs e)
        {
            // Update progress bar
            progressBarScan.Value = e.ProgressPercentage;
            
            // Update status
            UpdateStatus($"Scanning... {e.ProgressPercentage}% complete");
        }

        private void EnableScanningControls(bool enable)
        {
            // Enable or disable scanning controls based on process selection
            btnScan.Enabled = enable;
            cmbScanType.Enabled = enable;
            txtScanValue.Enabled = enable;
        }

        private void UpdateStatus(string message)
        {
            // Update status bar with message
            statusStrip.Items["statusLabel"].Text = message;
        }

        private void rainbowPcButton_Click(object sender, EventArgs e)
        {
            ShowProcessListForm();
        }
        
        private void OpenProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowProcessListForm();
        }
        
        private void ShowProcessListForm()
        {
            // Show process selection form
            using (ProcessSelectForm processSelectForm = new ProcessSelectForm())
            {
                if (processSelectForm.ShowDialog() == DialogResult.OK)
                {
                    ProcessSelected(processSelectForm.SelectedProcessId, processSelectForm.SelectedProcessName);
                }
            }
        }
        
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            if (!isScanning)
            {
                // Start new scan
                isScanning = true;
                btnScan.Text = "Stop";
                btnNextScan.Enabled = false;
                
                // Get scan parameters
                ScanType scanType = (ScanType)cmbScanType.SelectedItem;
                string scanValue = txtScanValue.Text;
                
                // Start scan
                memoryScanner.StartScan(scanType, scanValue);
                
                // Update status
                UpdateStatus("Scanning memory...");
            }
            else
            {
                // Stop scan
                memoryScanner.StopScan();
                isScanning = false;
                btnScan.Text = "New Scan";
                
                // Update status
                UpdateStatus("Scan stopped by user.");
            }
        }

        private void btnNextScan_Click(object sender, EventArgs e)
        {
            if (!isScanning)
            {
                // Start next scan
                isScanning = true;
                btnScan.Text = "Stop";
                
                // Get scan parameters
                ScanType scanType = (ScanType)cmbScanType.SelectedItem;
                string scanValue = txtScanValue.Text;
                
                // Start next scan
                memoryScanner.StartNextScan(scanType, scanValue);
                
                // Update status
                UpdateStatus("Performing next scan...");
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Initialize scan type combo box
            cmbScanType.Items.AddRange(Enum.GetValues(typeof(ScanType)).Cast<object>().ToArray());
            cmbScanType.SelectedIndex = 0;
            
            // Disable scanning controls until process is selected
            EnableScanningControls(false);
            
            // Update status
            UpdateStatus("Ready. Please select a process to begin.");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Clean up resources
            if (isScanning)
            {
                memoryScanner.StopScan();
            }
            
            // Shutdown Lua engine
            luaEngine.Shutdown();
            
            // Shutdown debugger
            debuggerManager.StopDebugging();
            
            // Shutdown core
            CheatEngineCore.Shutdown();
        }
        
        private void SetupMenuItems()
        {
            // Wire up existing menu items
            memoryViewerToolStripMenuItem.Click += MemoryViewerMenuItem_Click;
            disassemblerToolStripMenuItem.Click += DisassemblerMenuItem_Click;
            pointerScannerToolStripMenuItem.Click += PointerScanMenuItem_Click;
            autoAssemblerToolStripMenuItem.Click += AutoAssemblerMenuItem_Click;
            
            // Add Lua script menu item if it doesn't exist
            ToolStripMenuItem luaScriptMenuItem = new ToolStripMenuItem("Lua Script");
            luaScriptMenuItem.Click += LuaScriptMenuItem_Click;
            toolsToolStripMenuItem.DropDownItems.Add(luaScriptMenuItem);
            
            // Wire up file menu items
            openProcessToolStripMenuItem.Click += OpenProcessToolStripMenuItem_Click;
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
        }
        
        private void MemoryViewerMenuItem_Click(object sender, EventArgs e)
        {
            // Check if a process is attached
            if (CheatEngineCore.TargetProcess == null)
            {
                MessageBox.Show("Please attach to a process first.", "No Process", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Show memory viewer form
            MemoryViewerForm memoryViewerForm = new MemoryViewerForm();
            memoryViewerForm.Show();
        }
        
        private void DisassemblerMenuItem_Click(object sender, EventArgs e)
        {
            // Check if a process is attached
            if (CheatEngineCore.TargetProcess == null)
            {
                MessageBox.Show("Please attach to a process first.", "No Process", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Show disassembler form
            DisassemblerForm disassemblerForm = new DisassemblerForm();
            disassemblerForm.Show();
        }
        
        private void LuaScriptMenuItem_Click(object sender, EventArgs e)
        {
            // Show Lua script form
            LuaScriptForm luaScriptForm = new LuaScriptForm();
            luaScriptForm.Show();
        }
        
        private void PointerScanMenuItem_Click(object sender, EventArgs e)
        {
            // Check if a process is attached
            if (CheatEngineCore.TargetProcess == null)
            {
                MessageBox.Show("Please attach to a process first.", "No Process", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Show pointer scanner form
            PointerScannerForm pointerScannerForm = new PointerScannerForm();
            pointerScannerForm.Show();
        }
        
        private void AutoAssemblerMenuItem_Click(object sender, EventArgs e)
        {
            // Check if a process is attached
            if (CheatEngineCore.TargetProcess == null)
            {
                MessageBox.Show("Please attach to a process first.", "No Process", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Show auto assembler form
            AutoAssemblerForm autoAssemblerForm = new AutoAssemblerForm();
            autoAssemblerForm.Show();
        }
    }
}
