namespace ProcessFormDesigner;

partial class ProcessSelectForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.lblSelectProcess = new System.Windows.Forms.Label();
        this.btnRefresh = new System.Windows.Forms.Button();
        this.btnOK = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.processListView = new System.Windows.Forms.ListView();
        this.columnPID = new System.Windows.Forms.ColumnHeader();
        this.columnArch = new System.Windows.Forms.ColumnHeader();
        this.columnName = new System.Windows.Forms.ColumnHeader();
        this.imageList = new System.Windows.Forms.ImageList(this.components);
        this.txtFilter = new System.Windows.Forms.TextBox();
        this.lblFilter = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // 
        // lblSelectProcess
        // 
        this.lblSelectProcess.AutoSize = true;
        this.lblSelectProcess.Location = new System.Drawing.Point(12, 9);
        this.lblSelectProcess.Name = "lblSelectProcess";
        this.lblSelectProcess.Size = new System.Drawing.Size(89, 15);
        this.lblSelectProcess.TabIndex = 0;
        this.lblSelectProcess.Text = "Select Process:";
        // 
        // btnRefresh
        // 
        this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.btnRefresh.Location = new System.Drawing.Point(597, 27);
        this.btnRefresh.Name = "btnRefresh";
        this.btnRefresh.Size = new System.Drawing.Size(75, 23);
        this.btnRefresh.TabIndex = 2;
        this.btnRefresh.Text = "Refresh";
        this.btnRefresh.UseVisualStyleBackColor = true;
        // 
        // btnOK
        // 
        this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.btnOK.Location = new System.Drawing.Point(516, 426);
        this.btnOK.Name = "btnOK";
        this.btnOK.Size = new System.Drawing.Size(75, 23);
        this.btnOK.TabIndex = 3;
        this.btnOK.Text = "OK";
        this.btnOK.UseVisualStyleBackColor = true;
        // 
        // btnCancel
        // 
        this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnCancel.Location = new System.Drawing.Point(597, 426);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(75, 23);
        this.btnCancel.TabIndex = 4;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        // 
        // processListView
        // 
        this.processListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.processListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
        this.columnPID,
        this.columnArch,
        this.columnName});
        this.processListView.FullRowSelect = true;
        this.processListView.Location = new System.Drawing.Point(12, 56);
        this.processListView.MultiSelect = false;
        this.processListView.Name = "processListView";
        this.processListView.Size = new System.Drawing.Size(660, 364);
        this.processListView.SmallImageList = this.imageList;
        this.processListView.TabIndex = 5;
        this.processListView.UseCompatibleStateImageBehavior = false;
        this.processListView.View = System.Windows.Forms.View.Details;
        // 
        // columnPID
        // 
        this.columnPID.Text = "PID";
        this.columnPID.Width = 80;
        // 
        // columnArch
        // 
        this.columnArch.Text = "Architecture";
        this.columnArch.Width = 100;
        // 
        // columnName
        // 
        this.columnName.Text = "Process Name";
        this.columnName.Width = 450;
        // 
        // imageList
        // 
        this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
        this.imageList.ImageSize = new System.Drawing.Size(24, 24);
        this.imageList.TransparentColor = System.Drawing.Color.Transparent;
        // 
        // txtFilter
        // 
        this.txtFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.txtFilter.Location = new System.Drawing.Point(133, 27);
        this.txtFilter.Name = "txtFilter";
        this.txtFilter.Size = new System.Drawing.Size(458, 23);
        this.txtFilter.TabIndex = 7;
        // 
        // lblFilter
        // 
        this.lblFilter.AutoSize = true;
        this.lblFilter.Location = new System.Drawing.Point(12, 30);
        this.lblFilter.Name = "lblFilter";
        this.lblFilter.Size = new System.Drawing.Size(115, 15);
        this.lblFilter.TabIndex = 8;
        this.lblFilter.Text = "Search For Program:";
        // 
        // ProcessSelectForm
        // 
        this.AcceptButton = this.btnOK;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(684, 461);
        this.Controls.Add(this.lblFilter);
        this.Controls.Add(this.txtFilter);
        this.Controls.Add(this.processListView);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnOK);
        this.Controls.Add(this.btnRefresh);
        this.Controls.Add(this.lblSelectProcess);
        this.MinimizeBox = false;
        this.MinimumSize = new System.Drawing.Size(700, 500);
        this.Name = "ProcessSelectForm";
        this.ShowIcon = false;
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Select Process";
        this.Load += new System.EventHandler(this.ProcessSelectForm_Load);
        this.ResumeLayout(false);
        this.PerformLayout();
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
