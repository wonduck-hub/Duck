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
            splitContainerMain = new SplitContainer();
            TreeViewLoadedSARIF = new TreeView();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabelStatus = new ToolStripStatusLabel();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            loadSARIFToolStripMenuItem = new ToolStripMenuItem();
            loadDBToolStripMenuItem = new ToolStripMenuItem();
            exportDBToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            toolStrip1 = new ToolStrip();
            toolStripLabelDB = new ToolStripLabel();
            toolStripSeparator1 = new ToolStripSeparator();
            paneMain = new Panel();
            panelRight = new Panel();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            statusStrip1.SuspendLayout();
            menuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            paneMain.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainerMain
            // 
            splitContainerMain.Dock = DockStyle.Fill;
            splitContainerMain.Location = new Point(0, 0);
            splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            splitContainerMain.Panel1.Controls.Add(TreeViewLoadedSARIF);
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(panelRight);
            splitContainerMain.Size = new Size(1743, 973);
            splitContainerMain.SplitterDistance = 580;
            splitContainerMain.TabIndex = 1;
            // 
            // TreeViewLoadedSARIF
            // 
            TreeViewLoadedSARIF.Dock = DockStyle.Fill;
            TreeViewLoadedSARIF.Location = new Point(0, 0);
            TreeViewLoadedSARIF.Name = "TreeViewLoadedSARIF";
            TreeViewLoadedSARIF.Size = new Size(580, 973);
            TreeViewLoadedSARIF.TabIndex = 0;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(36, 36);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabelStatus });
            statusStrip1.Location = new Point(0, 1061);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1743, 48);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabelStatus
            // 
            toolStripStatusLabelStatus.Name = "toolStripStatusLabelStatus";
            toolStripStatusLabelStatus.Size = new Size(142, 37);
            toolStripStatusLabelStatus.Text = "SW Status";
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(36, 36);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, aboutToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1743, 45);
            menuStrip1.TabIndex = 3;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { loadSARIFToolStripMenuItem, loadDBToolStripMenuItem, exportDBToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(80, 41);
            fileToolStripMenuItem.Text = "File";
            // 
            // loadSARIFToolStripMenuItem
            // 
            loadSARIFToolStripMenuItem.Name = "loadSARIFToolStripMenuItem";
            loadSARIFToolStripMenuItem.Size = new Size(403, 48);
            loadSARIFToolStripMenuItem.Text = "Load SARIF";
            // 
            // loadDBToolStripMenuItem
            // 
            loadDBToolStripMenuItem.Name = "loadDBToolStripMenuItem";
            loadDBToolStripMenuItem.Size = new Size(403, 48);
            loadDBToolStripMenuItem.Text = "Load DB";
            // 
            // exportDBToolStripMenuItem
            // 
            exportDBToolStripMenuItem.Name = "exportDBToolStripMenuItem";
            exportDBToolStripMenuItem.Size = new Size(403, 48);
            exportDBToolStripMenuItem.Text = "Export DB";
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(114, 41);
            aboutToolStripMenuItem.Text = "About";
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(36, 36);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripLabelDB, toolStripSeparator1 });
            toolStrip1.Location = new Point(0, 45);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1743, 43);
            toolStrip1.TabIndex = 4;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabelDB
            // 
            toolStripLabelDB.Name = "toolStripLabelDB";
            toolStripLabelDB.Size = new Size(115, 37);
            toolStripLabelDB.Text = "DB Path";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 43);
            // 
            // paneMain
            // 
            paneMain.Controls.Add(splitContainerMain);
            paneMain.Dock = DockStyle.Fill;
            paneMain.Location = new Point(0, 88);
            paneMain.Name = "paneMain";
            paneMain.Size = new Size(1743, 973);
            paneMain.TabIndex = 5;
            // 
            // panelRight
            // 
            panelRight.Dock = DockStyle.Fill;
            panelRight.Location = new Point(0, 0);
            panelRight.Name = "panelRight";
            panelRight.Size = new Size(1159, 973);
            panelRight.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(15F, 37F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1743, 1109);
            Controls.Add(paneMain);
            Controls.Add(toolStrip1);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "MainForm";
            Text = "DuckSarifMonitor";
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            paneMain.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private SplitContainer splitContainerMain;
        private TreeView TreeViewLoadedSARIF;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabelStatus;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem loadSARIFToolStripMenuItem;
        private ToolStripMenuItem loadDBToolStripMenuItem;
        private ToolStripMenuItem exportDBToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStrip toolStrip1;
        private ToolStripLabel toolStripLabelDB;
        private ToolStripSeparator toolStripSeparator1;
        private Panel paneMain;
        private Panel panelRight;
    }
}
