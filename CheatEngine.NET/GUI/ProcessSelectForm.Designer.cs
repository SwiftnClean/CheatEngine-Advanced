namespace CheatEngine.NET.GUI
{
    partial class ProcessSelectForm
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
            components = new System.ComponentModel.Container();
            lblSelectProcess = new Label();
            btnRefresh = new Button();
            btnOK = new Button();
            btnCancel = new Button();
            processListView = new ListView();
            columnPID = new ColumnHeader();
            columnArch = new ColumnHeader();
            columnName = new ColumnHeader();
            imageList = new ImageList(components);
            txtFilter = new TextBox();
            lblFilter = new Label();
            SuspendLayout();
            // 
            // lblSelectProcess
            // 
            lblSelectProcess.AutoSize = true;
            lblSelectProcess.Location = new Point(12, 9);
            lblSelectProcess.Name = "lblSelectProcess";
            lblSelectProcess.Size = new Size(84, 15);
            lblSelectProcess.TabIndex = 1;
            lblSelectProcess.Text = "Select Process:";
            // 
            // btnRefresh
            // 
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Location = new Point(597, 27);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(75, 23);
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += BtnRefresh_Click;
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.Location = new Point(516, 426);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 23);
            btnOK.TabIndex = 3;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(597, 426);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // processListView
            // 
            processListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            processListView.Columns.AddRange(new ColumnHeader[] { columnPID, columnArch, columnName });
            processListView.FullRowSelect = true;
            processListView.Location = new Point(12, 86);
            processListView.MultiSelect = false;
            processListView.Name = "processListView";
            processListView.Size = new Size(660, 334);
            processListView.SmallImageList = imageList;
            processListView.TabIndex = 5;
            processListView.UseCompatibleStateImageBehavior = false;
            processListView.View = View.Details;
            processListView.SelectedIndexChanged += ProcessListView_SelectedIndexChanged;
            processListView.MouseDoubleClick += ProcessListView_MouseDoubleClick;
            // 
            // columnPID
            // 
            columnPID.Text = "PID";
            columnPID.Width = 80;
            // 
            // columnArch
            // 
            columnArch.Text = "Architecture";
            columnArch.Width = 100;
            // 
            // columnName
            // 
            columnName.Text = "Process Name";
            columnName.Width = 450;
            // 
            // imageList
            // 
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.ImageSize = new Size(24, 24);
            imageList.TransparentColor = Color.Transparent;
            // 
            // txtFilter
            // 
            txtFilter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFilter.Location = new Point(132, 27);
            txtFilter.Name = "txtFilter";
            txtFilter.Size = new Size(459, 23);
            txtFilter.TabIndex = 7;
            txtFilter.TextChanged += TxtFilter_TextChanged;
            // 
            // lblFilter
            // 
            lblFilter.AutoSize = true;
            lblFilter.Location = new Point(12, 30);
            lblFilter.Name = "lblFilter";
            lblFilter.Size = new Size(114, 15);
            lblFilter.TabIndex = 8;
            lblFilter.Text = "Search For Program:";
            // 
            // ProcessSelectForm
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(684, 461);
            Controls.Add(lblFilter);
            Controls.Add(txtFilter);
            Controls.Add(processListView);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(btnRefresh);
            Controls.Add(lblSelectProcess);
            MinimizeBox = false;
            MinimumSize = new Size(700, 500);
            Name = "ProcessSelectForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Select Process";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblSelectProcess;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ListView processListView;
        private System.Windows.Forms.ColumnHeader columnPID;
        private System.Windows.Forms.ColumnHeader columnArch;
        private System.Windows.Forms.ColumnHeader columnName;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.TextBox txtFilter;
        private System.Windows.Forms.Label lblFilter;
    }
}
