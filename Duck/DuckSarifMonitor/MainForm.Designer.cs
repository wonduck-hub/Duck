namespace DuckSarifMonitor
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            splitContainerMain = new SplitContainer();
            buttonSingleSarifDashboard = new Button();
            panelRight = new Panel();
            panelSingleDashboard = new Panel();
            dashboardLayout = new TableLayoutPanel();
            toolStrip2 = new ToolStrip();
            toolStripLabelFile = new ToolStripLabel();
            toolStripTextBoxSarifFilePath = new ToolStripTextBox();
            toolStripSeparator2 = new ToolStripSeparator();
            mSarifDashboardView = new SarifDashboardView();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabelStatus = new ToolStripStatusLabel();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            loadDBToolStripMenuItem = new ToolStripMenuItem();
            exportDBToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            toolStrip1 = new ToolStrip();
            toolStripLabelDB = new ToolStripLabel();
            toolStripTextBoxDbPath = new ToolStripTextBox();
            toolStripSeparator1 = new ToolStripSeparator();
            paneMain = new Panel();
            tabControl = new TabControl();
            tabPageSarifView = new TabPage();
            tabPageProjectView = new TabPage();
            toolStripButtonLoadSarifFile = new ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            panelRight.SuspendLayout();
            panelSingleDashboard.SuspendLayout();
            dashboardLayout.SuspendLayout();
            toolStrip2.SuspendLayout();
            statusStrip1.SuspendLayout();
            menuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            paneMain.SuspendLayout();
            tabControl.SuspendLayout();
            tabPageSarifView.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainerMain
            // 
            splitContainerMain.Dock = DockStyle.Fill;
            splitContainerMain.Location = new Point(1, 1);
            splitContainerMain.Margin = new Padding(1);
            splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            splitContainerMain.Panel1.Controls.Add(buttonSingleSarifDashboard);
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(panelRight);
            splitContainerMain.Size = new Size(1422, 613);
            splitContainerMain.SplitterDistance = 204;
            splitContainerMain.SplitterWidth = 2;
            splitContainerMain.TabIndex = 1;
            // 
            // buttonSingleSarifDashboard
            // 
            buttonSingleSarifDashboard.Dock = DockStyle.Top;
            buttonSingleSarifDashboard.Font = new Font("Segoe UI", 14F);
            buttonSingleSarifDashboard.Location = new Point(0, 0);
            buttonSingleSarifDashboard.Margin = new Padding(4, 3, 4, 3);
            buttonSingleSarifDashboard.Name = "buttonSingleSarifDashboard";
            buttonSingleSarifDashboard.Size = new Size(204, 57);
            buttonSingleSarifDashboard.TabIndex = 0;
            buttonSingleSarifDashboard.Text = "Sarif Dashboard";
            buttonSingleSarifDashboard.UseVisualStyleBackColor = true;
            buttonSingleSarifDashboard.Click += buttonSingleDashboard_Click;
            // 
            // panelRight
            // 
            panelRight.Controls.Add(panelSingleDashboard);
            panelRight.Dock = DockStyle.Fill;
            panelRight.Location = new Point(0, 0);
            panelRight.Margin = new Padding(1);
            panelRight.Name = "panelRight";
            panelRight.Size = new Size(1216, 613);
            panelRight.TabIndex = 0;
            // 
            // panelSingleDashboard
            // 
            panelSingleDashboard.Controls.Add(dashboardLayout);
            panelSingleDashboard.Dock = DockStyle.Fill;
            panelSingleDashboard.Location = new Point(0, 0);
            panelSingleDashboard.Margin = new Padding(4, 3, 4, 3);
            panelSingleDashboard.Name = "panelSingleDashboard";
            panelSingleDashboard.Size = new Size(1216, 613);
            panelSingleDashboard.TabIndex = 2;
            // 
            // dashboardLayout
            // 
            dashboardLayout.ColumnCount = 1;
            dashboardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            dashboardLayout.Controls.Add(toolStrip2, 0, 0);
            dashboardLayout.Controls.Add(mSarifDashboardView, 0, 1);
            dashboardLayout.Dock = DockStyle.Fill;
            dashboardLayout.Location = new Point(0, 0);
            dashboardLayout.Name = "dashboardLayout";
            dashboardLayout.RowCount = 2;
            dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            dashboardLayout.Size = new Size(1216, 613);
            dashboardLayout.TabIndex = 0;
            // 
            // toolStrip2
            // 
            toolStrip2.Items.AddRange(new ToolStripItem[] { toolStripLabelFile, toolStripTextBoxSarifFilePath, toolStripSeparator2, toolStripButtonLoadSarifFile });
            toolStrip2.Location = new Point(0, 0);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Size = new Size(1216, 25);
            toolStrip2.TabIndex = 2;
            toolStrip2.Text = "toolStrip2";
            // 
            // toolStripLabelFile
            // 
            toolStripLabelFile.Name = "toolStripLabelFile";
            toolStripLabelFile.Size = new Size(56, 22);
            toolStripLabelFile.Text = "SarifFile: ";
            // 
            // toolStripTextBoxSarifFilePath
            // 
            toolStripTextBoxSarifFilePath.Name = "toolStripTextBoxSarifFilePath";
            toolStripTextBoxSarifFilePath.ReadOnly = true;
            toolStripTextBoxSarifFilePath.Size = new Size(100, 25);
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // mSarifDashboardView
            // 
            mSarifDashboardView.BackColor = Color.FromArgb(246, 248, 250);
            mSarifDashboardView.Dock = DockStyle.Fill;
            mSarifDashboardView.Location = new Point(3, 28);
            mSarifDashboardView.Name = "mSarifDashboardView";
            mSarifDashboardView.Padding = new Padding(12);
            mSarifDashboardView.Size = new Size(1210, 582);
            mSarifDashboardView.TabIndex = 3;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(36, 36);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabelStatus });
            statusStrip1.Location = new Point(0, 692);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new Padding(0, 0, 7, 0);
            statusStrip1.Size = new Size(1432, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabelStatus
            // 
            toolStripStatusLabelStatus.Name = "toolStripStatusLabelStatus";
            toolStripStatusLabelStatus.Size = new Size(62, 17);
            toolStripStatusLabelStatus.Text = "SW Status";
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(36, 36);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, aboutToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(4, 1, 0, 1);
            menuStrip1.Size = new Size(1432, 24);
            menuStrip1.TabIndex = 3;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { loadDBToolStripMenuItem, exportDBToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 22);
            fileToolStripMenuItem.Text = "File";
            // 
            // loadDBToolStripMenuItem
            // 
            loadDBToolStripMenuItem.Name = "loadDBToolStripMenuItem";
            loadDBToolStripMenuItem.Size = new Size(180, 22);
            loadDBToolStripMenuItem.Text = "Load DB";
            // 
            // exportDBToolStripMenuItem
            // 
            exportDBToolStripMenuItem.Name = "exportDBToolStripMenuItem";
            exportDBToolStripMenuItem.Size = new Size(180, 22);
            exportDBToolStripMenuItem.Text = "Export DB";
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(52, 22);
            aboutToolStripMenuItem.Text = "About";
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(36, 36);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripLabelDB, toolStripTextBoxDbPath, toolStripSeparator1 });
            toolStrip1.Location = new Point(0, 24);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Padding = new Padding(0);
            toolStrip1.Size = new Size(1432, 25);
            toolStrip1.TabIndex = 4;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabelDB
            // 
            toolStripLabelDB.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            toolStripLabelDB.Name = "toolStripLabelDB";
            toolStripLabelDB.Size = new Size(55, 22);
            toolStripLabelDB.Text = "DB Path: ";
            // 
            // toolStripTextBoxDbPath
            // 
            toolStripTextBoxDbPath.Name = "toolStripTextBoxDbPath";
            toolStripTextBoxDbPath.ReadOnly = true;
            toolStripTextBoxDbPath.Size = new Size(100, 25);
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // paneMain
            // 
            paneMain.Controls.Add(tabControl);
            paneMain.Dock = DockStyle.Fill;
            paneMain.Location = new Point(0, 49);
            paneMain.Margin = new Padding(1);
            paneMain.Name = "paneMain";
            paneMain.Size = new Size(1432, 643);
            paneMain.TabIndex = 5;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPageSarifView);
            tabControl.Controls.Add(tabPageProjectView);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Margin = new Padding(1);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1432, 643);
            tabControl.TabIndex = 2;
            // 
            // tabPageSarifView
            // 
            tabPageSarifView.Controls.Add(splitContainerMain);
            tabPageSarifView.Location = new Point(4, 24);
            tabPageSarifView.Margin = new Padding(1);
            tabPageSarifView.Name = "tabPageSarifView";
            tabPageSarifView.Padding = new Padding(1);
            tabPageSarifView.Size = new Size(1424, 615);
            tabPageSarifView.TabIndex = 0;
            tabPageSarifView.Text = "SarifView";
            tabPageSarifView.UseVisualStyleBackColor = true;
            // 
            // tabPageProjectView
            // 
            tabPageProjectView.Location = new Point(4, 24);
            tabPageProjectView.Margin = new Padding(1);
            tabPageProjectView.Name = "tabPageProjectView";
            tabPageProjectView.Padding = new Padding(1);
            tabPageProjectView.Size = new Size(192, 72);
            tabPageProjectView.TabIndex = 1;
            tabPageProjectView.Text = "ProjectView";
            tabPageProjectView.UseVisualStyleBackColor = true;
            // 
            // toolStripButtonLoadSarifFile
            // 
            toolStripButtonLoadSarifFile.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButtonLoadSarifFile.Image = (Image)resources.GetObject("toolStripButtonLoadSarifFile.Image");
            toolStripButtonLoadSarifFile.ImageTransparentColor = Color.Magenta;
            toolStripButtonLoadSarifFile.Name = "toolStripButtonLoadSarifFile";
            toolStripButtonLoadSarifFile.Size = new Size(23, 22);
            toolStripButtonLoadSarifFile.Text = "Load Sarif";
            toolStripButtonLoadSarifFile.Click += loadSARIFToolStripMenuItem_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1432, 714);
            Controls.Add(paneMain);
            Controls.Add(toolStrip1);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MainMenuStrip = menuStrip1;
            Margin = new Padding(1);
            Name = "MainForm";
            Text = "DuckQualityMonitor";
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            panelRight.ResumeLayout(false);
            panelSingleDashboard.ResumeLayout(false);
            dashboardLayout.ResumeLayout(false);
            dashboardLayout.PerformLayout();
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            paneMain.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            tabPageSarifView.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private SplitContainer splitContainerMain;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabelStatus;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem loadDBToolStripMenuItem;
        private ToolStripMenuItem exportDBToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStrip toolStrip1;
        private ToolStripLabel toolStripLabelDB;
        private Panel paneMain;
        private Panel panelRight;
        private TabControl tabControl;
        private TabPage tabPageSarifView;
        private TabPage tabPageProjectView;
        private ToolStripTextBox toolStripTextBoxDbPath;
        private ToolStripSeparator toolStripSeparator1;
        private Button buttonSingleSarifDashboard;
        private Panel panelSingleDashboard;
        private TableLayoutPanel dashboardLayout;
        private SarifDashboardView mSarifDashboardView;
        private ToolStrip toolStrip2;
        private ToolStripLabel toolStripLabelFile;
        private ToolStripTextBox toolStripTextBoxSarifFilePath;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton toolStripButtonLoadSarifFile;
    }
}
