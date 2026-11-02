namespace DuckSarifMonitor
{
    internal sealed partial class SarifDashboardView
    {
        private Label mTitleLabel;
        private Label mMetadataLabel;
        private FlowLayoutPanel mSummaryPanel;
        private Button mTotalSummaryButton;
        private Button mErrorSummaryButton;
        private Button mWarningSummaryButton;
        private Button mNoteSummaryButton;
        private Button mOtherSummaryButton;
        private TextBox mSearchTextBox;
        private ComboBox mSeverityComboBox;
        private Button mClearFiltersButton;
        private ListView mTopRulesListView;
        private DataGridView mResultsGrid;
        private RichTextBox mDetailsTextBox;
        private Button mMetadataToggleButton;
        private Panel mMetadataPanel;
        private RichTextBox mMetadataTextBox;
        private Label mStatusLabel;
        private SeverityBarChart mSeverityChart;

        private void InitializeComponent()
        {
            rootLayout = new TableLayoutPanel();
            headerPanel = new Panel();
            mMetadataLabel = new Label();
            mTitleLabel = new Label();
            mSummaryPanel = new FlowLayoutPanel();
            mTotalSummaryButton = createSummaryButton("Total    0", string.Empty);
            mErrorSummaryButton = createSummaryButton("Errors    0", "Error");
            mWarningSummaryButton = createSummaryButton("Warnings    0", "Warning");
            mNoteSummaryButton = createSummaryButton("Notes    0", "Note");
            mOtherSummaryButton = createSummaryButton("Other    0", "Other");
            contentSplit = new SplitContainer();
            findingsLayout = new TableLayoutPanel();
            filterPanel = new FlowLayoutPanel();
            searchLabel = new Label();
            mSearchTextBox = new TextBox();
            mSeverityComboBox = new ComboBox();
            mClearFiltersButton = new Button();
            distributionGroup = new GroupBox();
            mSeverityChart = new SeverityBarChart();
            mResultsGrid = new DataGridView();
            mStatusLabel = new Label();
            detailsLayout = new TableLayoutPanel();
            topRulesGroup = new GroupBox();
            mTopRulesListView = new ListView();
            mMetadataToggleButton = new Button();
            selectedGroup = new GroupBox();
            selectedLayout = new TableLayoutPanel();
            mDetailsTextBox = new RichTextBox();
            mMetadataPanel = new Panel();
            mMetadataTextBox = new RichTextBox();
            rootLayout.SuspendLayout();
            headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)contentSplit).BeginInit();
            contentSplit.Panel1.SuspendLayout();
            contentSplit.Panel2.SuspendLayout();
            contentSplit.SuspendLayout();
            findingsLayout.SuspendLayout();
            filterPanel.SuspendLayout();
            distributionGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mResultsGrid).BeginInit();
            configureResultsGrid();
            detailsLayout.SuspendLayout();
            topRulesGroup.SuspendLayout();
            selectedGroup.SuspendLayout();
            selectedLayout.SuspendLayout();
            mMetadataPanel.SuspendLayout();
            SuspendLayout();
            // 
            // rootLayout
            // 
            rootLayout.ColumnCount = 1;
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            rootLayout.Controls.Add(headerPanel, 0, 0);
            rootLayout.Controls.Add(mSummaryPanel, 0, 1);
            rootLayout.Controls.Add(contentSplit, 0, 2);
            rootLayout.Dock = DockStyle.Fill;
            rootLayout.Location = new Point(12, 12);
            rootLayout.Name = "rootLayout";
            rootLayout.RowCount = 3;
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rootLayout.Size = new Size(1445, 858);
            rootLayout.TabIndex = 0;
            // 
            // headerPanel
            // 
            headerPanel.Controls.Add(mMetadataLabel);
            headerPanel.Controls.Add(mTitleLabel);
            headerPanel.Dock = DockStyle.Fill;
            headerPanel.Location = new Point(3, 3);
            headerPanel.Name = "headerPanel";
            headerPanel.Padding = new Padding(8, 2, 8, 4);
            headerPanel.Size = new Size(1439, 58);
            headerPanel.TabIndex = 0;
            // 
            // mMetadataLabel
            // 
            mMetadataLabel.AutoEllipsis = true;
            mMetadataLabel.Dock = DockStyle.Fill;
            mMetadataLabel.ForeColor = Color.FromArgb(87, 96, 106);
            mMetadataLabel.Location = new Point(8, 32);
            mMetadataLabel.Name = "mMetadataLabel";
            mMetadataLabel.Size = new Size(1423, 22);
            mMetadataLabel.TabIndex = 0;
            mMetadataLabel.Text = "Load a SARIF file to inspect its findings.";
            // 
            // mTitleLabel
            // 
            mTitleLabel.Dock = DockStyle.Top;
            mTitleLabel.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            mTitleLabel.Location = new Point(8, 2);
            mTitleLabel.Name = "mTitleLabel";
            mTitleLabel.Size = new Size(1423, 30);
            mTitleLabel.TabIndex = 1;
            mTitleLabel.Text = "Single SARIF report";
            // 
            // mSummaryPanel
            // 
            mSummaryPanel.AutoScroll = true;
            mSummaryPanel.Dock = DockStyle.Fill;
            mSummaryPanel.Location = new Point(3, 67);
            mSummaryPanel.Name = "mSummaryPanel";
            mSummaryPanel.Padding = new Padding(4, 4, 4, 8);
            mSummaryPanel.Size = new Size(1439, 76);
            mSummaryPanel.TabIndex = 1;
            mSummaryPanel.WrapContents = false;
            mSummaryPanel.Controls.Add(mTotalSummaryButton);
            mSummaryPanel.Controls.Add(mErrorSummaryButton);
            mSummaryPanel.Controls.Add(mWarningSummaryButton);
            mSummaryPanel.Controls.Add(mNoteSummaryButton);
            mSummaryPanel.Controls.Add(mOtherSummaryButton);
            // 
            // contentSplit
            // 
            contentSplit.Dock = DockStyle.Fill;
            contentSplit.Location = new Point(3, 149);
            contentSplit.Name = "contentSplit";
            // 
            // contentSplit.Panel1
            // 
            contentSplit.Panel1.Controls.Add(findingsLayout);
            contentSplit.Panel1MinSize = 440;
            // 
            // contentSplit.Panel2
            // 
            contentSplit.Panel2.Controls.Add(detailsLayout);
            contentSplit.Panel2MinSize = 280;
            contentSplit.Size = new Size(1439, 706);
            contentSplit.SplitterDistance = 633;
            contentSplit.TabIndex = 2;
            // 
            // findingsLayout
            // 
            findingsLayout.ColumnCount = 1;
            findingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            findingsLayout.Controls.Add(filterPanel, 0, 0);
            findingsLayout.Controls.Add(distributionGroup, 0, 1);
            findingsLayout.Controls.Add(mResultsGrid, 0, 2);
            findingsLayout.Controls.Add(mStatusLabel, 0, 3);
            findingsLayout.Dock = DockStyle.Fill;
            findingsLayout.Location = new Point(0, 0);
            findingsLayout.Name = "findingsLayout";
            findingsLayout.RowCount = 4;
            findingsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            findingsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 74F));
            findingsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            findingsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            findingsLayout.Size = new Size(633, 706);
            findingsLayout.TabIndex = 0;
            // 
            // filterPanel
            // 
            filterPanel.Controls.Add(searchLabel);
            filterPanel.Controls.Add(mSearchTextBox);
            filterPanel.Controls.Add(mSeverityComboBox);
            filterPanel.Controls.Add(mClearFiltersButton);
            filterPanel.Dock = DockStyle.Fill;
            filterPanel.Location = new Point(3, 3);
            filterPanel.Name = "filterPanel";
            filterPanel.Padding = new Padding(0, 4, 0, 0);
            filterPanel.Size = new Size(627, 34);
            filterPanel.TabIndex = 0;
            filterPanel.WrapContents = false;
            // 
            // searchLabel
            // 
            searchLabel.AutoSize = true;
            searchLabel.Location = new Point(4, 11);
            searchLabel.Margin = new Padding(4, 7, 4, 0);
            searchLabel.Name = "searchLabel";
            searchLabel.Size = new Size(43, 15);
            searchLabel.TabIndex = 0;
            searchLabel.Text = "Search";
            // 
            // mSearchTextBox
            // 
            mSearchTextBox.Location = new Point(53, 7);
            mSearchTextBox.Margin = new Padding(2, 3, 8, 0);
            mSearchTextBox.Name = "mSearchTextBox";
            mSearchTextBox.PlaceholderText = "Rule, message, or file";
            mSearchTextBox.Size = new Size(210, 23);
            mSearchTextBox.TabIndex = 1;
            // 
            // mSeverityComboBox
            // 
            mSeverityComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            mSeverityComboBox.Items.AddRange(new object[] { "Error", "Warning", "Note", "Other" });
            mSeverityComboBox.Location = new Point(273, 7);
            mSeverityComboBox.Margin = new Padding(2, 3, 8, 0);
            mSeverityComboBox.Name = "mSeverityComboBox";
            mSeverityComboBox.Size = new Size(128, 23);
            mSeverityComboBox.TabIndex = 2;
            // 
            // mClearFiltersButton
            // 
            mClearFiltersButton.AutoSize = true;
            mClearFiltersButton.Location = new Point(411, 6);
            mClearFiltersButton.Margin = new Padding(2, 2, 2, 0);
            mClearFiltersButton.Name = "mClearFiltersButton";
            mClearFiltersButton.Size = new Size(75, 25);
            mClearFiltersButton.TabIndex = 3;
            mClearFiltersButton.Text = "Clear";
            // 
            // distributionGroup
            // 
            distributionGroup.Controls.Add(mSeverityChart);
            distributionGroup.Dock = DockStyle.Fill;
            distributionGroup.Location = new Point(3, 43);
            distributionGroup.Name = "distributionGroup";
            distributionGroup.Padding = new Padding(8, 4, 8, 4);
            distributionGroup.Size = new Size(627, 68);
            distributionGroup.TabIndex = 1;
            distributionGroup.TabStop = false;
            distributionGroup.Text = "Severity distribution";
            // 
            // mSeverityChart
            // 
            mSeverityChart.BackColor = Color.White;
            mSeverityChart.Dock = DockStyle.Fill;
            mSeverityChart.Location = new Point(8, 20);
            mSeverityChart.MinimumSize = new Size(200, 30);
            mSeverityChart.Name = "mSeverityChart";
            mSeverityChart.Size = new Size(611, 44);
            mSeverityChart.TabIndex = 0;
            // 
            // mResultsGrid
            // 
            mResultsGrid.Location = new Point(3, 117);
            mResultsGrid.Name = "mResultsGrid";
            mResultsGrid.Size = new Size(240, 150);
            mResultsGrid.TabIndex = 2;
            // 
            // mStatusLabel
            // 
            mStatusLabel.Dock = DockStyle.Fill;
            mStatusLabel.ForeColor = Color.FromArgb(87, 96, 106);
            mStatusLabel.Location = new Point(3, 686);
            mStatusLabel.Name = "mStatusLabel";
            mStatusLabel.Size = new Size(627, 20);
            mStatusLabel.TabIndex = 3;
            mStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // detailsLayout
            // 
            detailsLayout.ColumnCount = 1;
            detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            detailsLayout.Controls.Add(topRulesGroup, 0, 0);
            detailsLayout.Controls.Add(mMetadataToggleButton, 0, 1);
            detailsLayout.Controls.Add(selectedGroup, 0, 2);
            detailsLayout.Dock = DockStyle.Fill;
            detailsLayout.Location = new Point(0, 0);
            detailsLayout.Name = "detailsLayout";
            detailsLayout.RowCount = 3;
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 43F));
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 57F));
            detailsLayout.Size = new Size(802, 706);
            detailsLayout.TabIndex = 0;
            // 
            // topRulesGroup
            // 
            topRulesGroup.Controls.Add(mTopRulesListView);
            topRulesGroup.Dock = DockStyle.Fill;
            topRulesGroup.Location = new Point(3, 3);
            topRulesGroup.Name = "topRulesGroup";
            topRulesGroup.Padding = new Padding(6);
            topRulesGroup.Size = new Size(796, 284);
            topRulesGroup.TabIndex = 0;
            topRulesGroup.TabStop = false;
            topRulesGroup.Text = "Top rules";
            // 
            // mTopRulesListView
            // 
            mTopRulesListView.Dock = DockStyle.Fill;
            mTopRulesListView.FullRowSelect = true;
            mTopRulesListView.Location = new Point(6, 22);
            mTopRulesListView.Name = "mTopRulesListView";
            mTopRulesListView.Size = new Size(784, 256);
            mTopRulesListView.TabIndex = 0;
            mTopRulesListView.UseCompatibleStateImageBehavior = false;
            mTopRulesListView.View = View.Details;
            // 
            // mMetadataToggleButton
            // 
            mMetadataToggleButton.Dock = DockStyle.Fill;
            mMetadataToggleButton.Enabled = false;
            mMetadataToggleButton.FlatAppearance.BorderSize = 0;
            mMetadataToggleButton.FlatStyle = FlatStyle.Flat;
            mMetadataToggleButton.Location = new Point(3, 293);
            mMetadataToggleButton.Name = "mMetadataToggleButton";
            mMetadataToggleButton.Size = new Size(796, 24);
            mMetadataToggleButton.TabIndex = 1;
            mMetadataToggleButton.Text = "Show rule metadata";
            mMetadataToggleButton.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // selectedGroup
            // 
            selectedGroup.Controls.Add(selectedLayout);
            selectedGroup.Dock = DockStyle.Fill;
            selectedGroup.Location = new Point(3, 323);
            selectedGroup.Name = "selectedGroup";
            selectedGroup.Padding = new Padding(8);
            selectedGroup.Size = new Size(796, 380);
            selectedGroup.TabIndex = 2;
            selectedGroup.TabStop = false;
            selectedGroup.Text = "Selected finding";
            // 
            // selectedLayout
            // 
            selectedLayout.ColumnCount = 1;
            selectedLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            selectedLayout.Controls.Add(mDetailsTextBox, 0, 0);
            selectedLayout.Controls.Add(mMetadataPanel, 0, 1);
            selectedLayout.Dock = DockStyle.Fill;
            selectedLayout.Location = new Point(8, 24);
            selectedLayout.Name = "selectedLayout";
            selectedLayout.RowCount = 2;
            selectedLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            selectedLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            selectedLayout.Size = new Size(780, 348);
            selectedLayout.TabIndex = 0;
            // 
            // mDetailsTextBox
            // 
            mDetailsTextBox.BackColor = SystemColors.Window;
            mDetailsTextBox.BorderStyle = BorderStyle.None;
            mDetailsTextBox.Dock = DockStyle.Fill;
            mDetailsTextBox.Location = new Point(3, 3);
            mDetailsTextBox.Name = "mDetailsTextBox";
            mDetailsTextBox.ReadOnly = true;
            mDetailsTextBox.Size = new Size(774, 242);
            mDetailsTextBox.TabIndex = 0;
            mDetailsTextBox.Text = "Select a finding to view its message and location.";
            // 
            // mMetadataPanel
            // 
            mMetadataPanel.Controls.Add(mMetadataTextBox);
            mMetadataPanel.Dock = DockStyle.Fill;
            mMetadataPanel.Location = new Point(3, 251);
            mMetadataPanel.Name = "mMetadataPanel";
            mMetadataPanel.Size = new Size(774, 94);
            mMetadataPanel.TabIndex = 1;
            mMetadataPanel.Visible = false;
            // 
            // mMetadataTextBox
            // 
            mMetadataTextBox.BorderStyle = BorderStyle.FixedSingle;
            mMetadataTextBox.Dock = DockStyle.Fill;
            mMetadataTextBox.Location = new Point(0, 0);
            mMetadataTextBox.Name = "mMetadataTextBox";
            mMetadataTextBox.ReadOnly = true;
            mMetadataTextBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            mMetadataTextBox.Size = new Size(774, 94);
            mMetadataTextBox.TabIndex = 0;
            mMetadataTextBox.Text = "";
            // 
            // SarifDashboardView
            // 
            BackColor = Color.FromArgb(246, 248, 250);
            Controls.Add(rootLayout);
            Name = "SarifDashboardView";
            Padding = new Padding(12);
            Size = new Size(1469, 882);
            rootLayout.ResumeLayout(false);
            headerPanel.ResumeLayout(false);
            contentSplit.Panel1.ResumeLayout(false);
            contentSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)contentSplit).EndInit();
            contentSplit.ResumeLayout(false);
            findingsLayout.ResumeLayout(false);
            filterPanel.ResumeLayout(false);
            filterPanel.PerformLayout();
            distributionGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mResultsGrid).EndInit();
            detailsLayout.ResumeLayout(false);
            topRulesGroup.ResumeLayout(false);
            selectedGroup.ResumeLayout(false);
            selectedLayout.ResumeLayout(false);
            mMetadataPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        private static Button createSummaryButton(string text, string severityFilter)
        {
            Button summaryButton = new Button();
            summaryButton.Width = 146;
            summaryButton.Height = 66;
            summaryButton.Margin = new Padding(4, 2, 8, 2);
            summaryButton.FlatStyle = FlatStyle.Flat;
            summaryButton.FlatAppearance.BorderColor = Color.FromArgb(208, 215, 222);
            summaryButton.BackColor = Color.White;
            summaryButton.TextAlign = ContentAlignment.MiddleLeft;
            summaryButton.Padding = new Padding(8, 0, 0, 0);
            summaryButton.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            summaryButton.ForeColor = Color.FromArgb(31, 35, 40);
            summaryButton.Text = text;
            summaryButton.Tag = severityFilter;
            return summaryButton;
        }

        private void configureResultsGrid()
        {
            mResultsGrid.AllowUserToAddRows = false;
            mResultsGrid.AllowUserToDeleteRows = false;
            mResultsGrid.AllowUserToResizeRows = false;
            mResultsGrid.AutoGenerateColumns = false;
            mResultsGrid.BackgroundColor = SystemColors.Window;
            mResultsGrid.BorderStyle = BorderStyle.FixedSingle;
            mResultsGrid.Dock = DockStyle.Fill;
            mResultsGrid.MultiSelect = false;
            mResultsGrid.ReadOnly = true;
            mResultsGrid.RowHeadersVisible = false;
            mResultsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            mResultsGrid.Columns.Add(createGridColumn("Severity", "Severity", "Level", 82));
            mResultsGrid.Columns[0].Frozen = true;
            mResultsGrid.Columns.Add(createGridColumn("RuleId", "Rule", "RuleId", 120));

            DataGridViewTextBoxColumn messageColumn = createGridColumn("Message", "Message", "Message", 180);
            messageColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            mResultsGrid.Columns.Add(messageColumn);
            mResultsGrid.Columns.Add(createGridColumn("File", "File", "File", 170));
            mResultsGrid.Columns.Add(createGridColumn("Line", "Line", "Line", 58));
        }

        private static DataGridViewTextBoxColumn createGridColumn(
            string name,
            string headerText,
            string dataPropertyName,
            int width)
        {
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.Name = name;
            column.HeaderText = headerText;
            column.DataPropertyName = dataPropertyName;
            column.Width = width;
            return column;
        }

        private TableLayoutPanel rootLayout;
        private Panel headerPanel;
        private SplitContainer contentSplit;
        private TableLayoutPanel findingsLayout;
        private FlowLayoutPanel filterPanel;
        private Label searchLabel;
        private GroupBox distributionGroup;
        private TableLayoutPanel detailsLayout;
        private GroupBox topRulesGroup;
        private GroupBox selectedGroup;
        private TableLayoutPanel selectedLayout;
    }
}
