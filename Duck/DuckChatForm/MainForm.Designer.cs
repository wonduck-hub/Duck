namespace DuckChatForm
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
            if (disposing)
            {
                disposeClient();
            }
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
            TreeNode treeNode1 = new TreeNode("Node0");
            TreeNode treeNode2 = new TreeNode("Node1");
            TreeNode treeNode3 = new TreeNode("Node2");
            TreeNode treeNode4 = new TreeNode("Node3");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            loginToolStripMenuItem = new ToolStripMenuItem();
            changeLoginToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            panelSend = new Panel();
            buttonAddFile = new Button();
            buttonSend = new Button();
            textBoxMessage = new TextBox();
            panelLeft = new Panel();
            treeView1 = new TreeView();
            buttonSetting = new Button();
            listBoxMessages = new ListBox();
            menuStrip1.SuspendLayout();
            panelSend.SuspendLayout();
            panelLeft.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(36, 36);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, loginToolStripMenuItem, aboutToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1597, 45);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(80, 41);
            fileToolStripMenuItem.Text = "File";
            // 
            // loginToolStripMenuItem
            // 
            loginToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { changeLoginToolStripMenuItem });
            loginToolStripMenuItem.Name = "loginToolStripMenuItem";
            loginToolStripMenuItem.Size = new Size(107, 41);
            loginToolStripMenuItem.Text = "Login";
            // 
            // changeLoginToolStripMenuItem
            // 
            changeLoginToolStripMenuItem.Name = "changeLoginToolStripMenuItem";
            changeLoginToolStripMenuItem.Size = new Size(331, 48);
            changeLoginToolStripMenuItem.Text = "Change login";
            changeLoginToolStripMenuItem.Click += changeLoginToolStripMenuItem_Click;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(114, 41);
            aboutToolStripMenuItem.Text = "About";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // panelSend
            // 
            panelSend.Controls.Add(buttonAddFile);
            panelSend.Controls.Add(buttonSend);
            panelSend.Controls.Add(textBoxMessage);
            panelSend.Dock = DockStyle.Bottom;
            panelSend.Location = new Point(435, 835);
            panelSend.Name = "panelSend";
            panelSend.Size = new Size(1162, 75);
            panelSend.TabIndex = 1;
            // 
            // buttonAddFile
            // 
            buttonAddFile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonAddFile.Location = new Point(12, 11);
            buttonAddFile.Name = "buttonAddFile";
            buttonAddFile.Size = new Size(169, 52);
            buttonAddFile.TabIndex = 2;
            buttonAddFile.Text = "Add...";
            buttonAddFile.UseVisualStyleBackColor = true;
            // 
            // buttonSend
            // 
            buttonSend.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonSend.Location = new Point(981, 11);
            buttonSend.Name = "buttonSend";
            buttonSend.Size = new Size(169, 52);
            buttonSend.TabIndex = 1;
            buttonSend.Text = "Send";
            buttonSend.UseVisualStyleBackColor = true;
            buttonSend.Click += buttonSend_Click;
            // 
            // textBoxMessage
            // 
            textBoxMessage.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBoxMessage.Location = new Point(187, 20);
            textBoxMessage.Name = "textBoxMessage";
            textBoxMessage.Size = new Size(788, 43);
            textBoxMessage.TabIndex = 0;
            // 
            // panelLeft
            // 
            panelLeft.Controls.Add(treeView1);
            panelLeft.Controls.Add(buttonSetting);
            panelLeft.Dock = DockStyle.Left;
            panelLeft.Location = new Point(0, 45);
            panelLeft.Name = "panelLeft";
            panelLeft.Size = new Size(435, 865);
            panelLeft.TabIndex = 2;
            // 
            // treeView1
            // 
            treeView1.Dock = DockStyle.Fill;
            treeView1.Location = new Point(0, 0);
            treeView1.Name = "treeView1";
            treeNode1.Name = "Node0";
            treeNode1.Text = "Node0";
            treeNode2.Name = "Node1";
            treeNode2.Text = "Node1";
            treeNode3.Name = "Node2";
            treeNode3.Text = "Node2";
            treeNode4.Name = "Node3";
            treeNode4.Text = "Node3";
            treeView1.Nodes.AddRange(new TreeNode[] { treeNode1, treeNode2, treeNode3, treeNode4 });
            treeView1.Size = new Size(435, 790);
            treeView1.TabIndex = 1;
            // 
            // buttonSetting
            // 
            buttonSetting.Dock = DockStyle.Bottom;
            buttonSetting.Location = new Point(0, 790);
            buttonSetting.Name = "buttonSetting";
            buttonSetting.Size = new Size(435, 75);
            buttonSetting.TabIndex = 0;
            buttonSetting.Text = "Setting";
            buttonSetting.UseVisualStyleBackColor = true;
            //
            // listBoxMessages
            //
            listBoxMessages.Dock = DockStyle.Fill;
            listBoxMessages.FormattingEnabled = true;
            listBoxMessages.Name = "listBoxMessages";
            listBoxMessages.TabIndex = 3;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(15F, 37F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1597, 910);
            Controls.Add(listBoxMessages);
            Controls.Add(panelSend);
            Controls.Add(panelLeft);
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DuckChatForm";
            Load += Form1_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            panelSend.ResumeLayout(false);
            panelSend.PerformLayout();
            panelLeft.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem loginToolStripMenuItem;
        private ToolStripMenuItem changeLoginToolStripMenuItem;
        private Panel panelSend;
        private Button buttonSend;
        private TextBox textBoxMessage;
        private Button buttonAddFile;
        private Panel panelLeft;
        private Button buttonSetting;
        private TreeView treeView1;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ListBox listBoxMessages;
    }
}
