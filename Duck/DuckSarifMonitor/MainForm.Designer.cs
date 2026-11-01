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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            splitContainerMain = new SplitContainer();
            TreeViewLoadedSARIF = new TreeView();
            panelRight = new Panel();
            chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
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
            tabControl = new TabControl();
            tabPageSingleView = new TabPage();
            tabPageProjectView = new TabPage();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            panelRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chart1).BeginInit();
            statusStrip1.SuspendLayout();
            menuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            paneMain.SuspendLayout();
            tabControl.SuspendLayout();
            tabPageSingleView.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainerMain
            // 
            splitContainerMain.Dock = DockStyle.Fill;
            splitContainerMain.Location = new Point(1, 1);
            splitContainerMain.Margin = new Padding(1, 1, 1, 1);
            splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            splitContainerMain.Panel1.Controls.Add(TreeViewLoadedSARIF);
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(panelRight);
            splitContainerMain.Size = new Size(1422, 613);
            splitContainerMain.SplitterDistance = 204;
            splitContainerMain.SplitterWidth = 2;
            splitContainerMain.TabIndex = 1;
            // 
            // TreeViewLoadedSARIF
            // 
            TreeViewLoadedSARIF.Dock = DockStyle.Fill;
            TreeViewLoadedSARIF.Location = new Point(0, 0);
            TreeViewLoadedSARIF.Margin = new Padding(1, 1, 1, 1);
            TreeViewLoadedSARIF.Name = "TreeViewLoadedSARIF";
            TreeViewLoadedSARIF.Size = new Size(204, 613);
            TreeViewLoadedSARIF.TabIndex = 0;
            // 
            // panelRight
            // 
            panelRight.Controls.Add(chart1);
            panelRight.Dock = DockStyle.Fill;
            panelRight.Location = new Point(0, 0);
            panelRight.Margin = new Padding(1, 1, 1, 1);
            panelRight.Name = "panelRight";
            panelRight.Size = new Size(1216, 613);
            panelRight.TabIndex = 0;
            // 
            // chart1
            // 
            chartArea1.Name = "ChartArea1";
            chart1.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            chart1.Legends.Add(legend1);
            chart1.Location = new Point(1, 1);
            chart1.Margin = new Padding(1, 1, 1, 1);
            chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            chart1.Series.Add(series1);
            chart1.Size = new Size(315, 274);
            chart1.TabIndex = 0;
            chart1.Text = "chart1";
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
            menuStrip1.Padding = new Padding(3, 1, 0, 1);
            menuStrip1.Size = new Size(1432, 24);
            menuStrip1.TabIndex = 3;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { loadSARIFToolStripMenuItem, loadDBToolStripMenuItem, exportDBToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 22);
            fileToolStripMenuItem.Text = "File";
            // 
            // loadSARIFToolStripMenuItem
            // 
            loadSARIFToolStripMenuItem.Name = "loadSARIFToolStripMenuItem";
            loadSARIFToolStripMenuItem.Size = new Size(135, 22);
            loadSARIFToolStripMenuItem.Text = "Load SARIF";
            // 
            // loadDBToolStripMenuItem
            // 
            loadDBToolStripMenuItem.Name = "loadDBToolStripMenuItem";
            loadDBToolStripMenuItem.Size = new Size(135, 22);
            loadDBToolStripMenuItem.Text = "Load DB";
            // 
            // exportDBToolStripMenuItem
            // 
            exportDBToolStripMenuItem.Name = "exportDBToolStripMenuItem";
            exportDBToolStripMenuItem.Size = new Size(135, 22);
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
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripLabelDB, toolStripSeparator1 });
            toolStrip1.Location = new Point(0, 24);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Padding = new Padding(0, 0, 0, 0);
            toolStrip1.Size = new Size(1432, 25);
            toolStrip1.TabIndex = 4;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabelDB
            // 
            toolStripLabelDB.Name = "toolStripLabelDB";
            toolStripLabelDB.Size = new Size(51, 22);
            toolStripLabelDB.Text = "DB Path";
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
            paneMain.Margin = new Padding(1, 1, 1, 1);
            paneMain.Name = "paneMain";
            paneMain.Size = new Size(1432, 643);
            paneMain.TabIndex = 5;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPageSingleView);
            tabControl.Controls.Add(tabPageProjectView);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Margin = new Padding(1, 1, 1, 1);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1432, 643);
            tabControl.TabIndex = 2;
            // 
            // tabPageSingleView
            // 
            tabPageSingleView.Controls.Add(splitContainerMain);
            tabPageSingleView.Location = new Point(4, 24);
            tabPageSingleView.Margin = new Padding(1, 1, 1, 1);
            tabPageSingleView.Name = "tabPageSingleView";
            tabPageSingleView.Padding = new Padding(1, 1, 1, 1);
            tabPageSingleView.Size = new Size(1424, 615);
            tabPageSingleView.TabIndex = 0;
            tabPageSingleView.Text = "SingleView";
            tabPageSingleView.UseVisualStyleBackColor = true;
            // 
            // tabPageProjectView
            // 
            tabPageProjectView.Location = new Point(4, 24);
            tabPageProjectView.Margin = new Padding(1, 1, 1, 1);
            tabPageProjectView.Name = "tabPageProjectView";
            tabPageProjectView.Padding = new Padding(1, 1, 1, 1);
            tabPageProjectView.Size = new Size(805, 366);
            tabPageProjectView.TabIndex = 1;
            tabPageProjectView.Text = "ProjectView";
            tabPageProjectView.UseVisualStyleBackColor = true;
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
            MainMenuStrip = menuStrip1;
            Margin = new Padding(1, 1, 1, 1);
            Name = "MainForm";
            Text = "DuckSarifMonitor";
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            panelRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)chart1).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            paneMain.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            tabPageSingleView.ResumeLayout(false);
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
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private TabControl tabControl;
        private TabPage tabPageSingleView;
        private TabPage tabPageProjectView;
    }
}
