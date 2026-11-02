using System.Drawing.Drawing2D;
using System.Text;
using System.Text.Json;

namespace DuckSarifMonitor
{
    internal sealed partial class SarifDashboardView : UserControl
    {
        private const string ALL_SEVERITIES = "All severities";

        private readonly List<FindingRow> mFindings;
        private string mActiveRuleFilter;

        public SarifDashboardView()
        {
            InitializeComponent();
            mFindings = new List<FindingRow>();
            mActiveRuleFilter = string.Empty;

            mSearchTextBox.TextChanged += searchTextBox_TextChanged;
            mSeverityComboBox.SelectedIndexChanged += severityComboBox_SelectedIndexChanged;
            mClearFiltersButton.Click += clearFiltersButton_Click;
            mTotalSummaryButton.Click += summaryCard_Click;
            mErrorSummaryButton.Click += summaryCard_Click;
            mWarningSummaryButton.Click += summaryCard_Click;
            mNoteSummaryButton.Click += summaryCard_Click;
            mOtherSummaryButton.Click += summaryCard_Click;
            mTopRulesListView.SelectedIndexChanged += topRulesListView_SelectedIndexChanged;
            mResultsGrid.SelectionChanged += resultsGrid_SelectionChanged;
            mMetadataToggleButton.Click += metadataToggleButton_Click;

            mSeverityChart.SeveritySelected += distributionChart_SeveritySelected;
            ShowEmptyState();
        }

        public void LoadSarif(string sarifPath)
        {
            List<FindingRow> findings = new List<FindingRow>();
            Dictionary<string, int> ruleCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            string version;
            string toolName = "Unknown tool";
            string toolVersion = string.Empty;
            DateTimeOffset? generatedAt = null;

            using (JsonDocument document = JsonDocument.Parse(File.ReadAllText(sarifPath)))
            {
                JsonElement root = document.RootElement;
                version = GetString(root, "version", "Unknown");
                if (!TryGetArray(root, "runs", out JsonElement runs))
                {
                    throw new InvalidDataException("SARIF document must contain a runs array.");
                }

                foreach (JsonElement run in runs.EnumerateArray())
                {
                    RuleCatalog rules = ReadRules(run);
                    if (TryGetProperty(run, "tool", out JsonElement tool)
                        && TryGetProperty(tool, "driver", out JsonElement driver))
                    {
                        toolName = GetString(driver, "name", toolName);
                        toolVersion = GetString(driver, "version", toolVersion);
                    }

                    ReadInvocationTime(run, ref generatedAt);
                    if (!TryGetArray(run, "results", out JsonElement results))
                    {
                        continue;
                    }

                    foreach (JsonElement result in results.EnumerateArray())
                    {
                        FindingRow finding = ReadFinding(result, rules);
                        findings.Add(finding);
                        ruleCounts.TryGetValue(finding.RuleId, out int ruleCount);
                        ruleCounts[finding.RuleId] = ruleCount + 1;
                    }
                }
            }

            mFindings.Clear();
            mFindings.AddRange(findings);
            mActiveRuleFilter = string.Empty;
            mSearchTextBox.Clear();
            mSeverityComboBox.SelectedIndex = 0;
            string fileName = Path.GetFileName(sarifPath);
            string directoryName = Path.GetFileName(Path.GetDirectoryName(sarifPath)) ?? string.Empty;
            string projectName = directoryName.StartsWith("DuckChat_", StringComparison.OrdinalIgnoreCase)
                ? directoryName
                : Path.GetFileNameWithoutExtension(sarifPath);
            DateTimeOffset reportTime = generatedAt ?? new DateTimeOffset(File.GetLastWriteTimeUtc(sarifPath));
            mTitleLabel.Text = projectName;
            mMetadataLabel.Text = string.Format(
                "File: {0}    Tool: {1}{2}    Generated: {3:u}    SARIF: {4}",
                fileName,
                toolName,
                string.IsNullOrWhiteSpace(toolVersion) ? string.Empty : " " + toolVersion,
                reportTime,
                version);
            UpdateSummary(findings);
            UpdateTopRules(ruleCounts);
            UpdateDistribution(findings);
            ApplyFilters();
        }

        private static RuleCatalog ReadRules(JsonElement run)
        {
            RuleCatalog rules = new RuleCatalog();
            if (!TryGetProperty(run, "tool", out JsonElement tool)
                || !TryGetProperty(tool, "driver", out JsonElement driver)
                || !TryGetArray(driver, "rules", out JsonElement ruleArray))
            {
                return rules;
            }

            foreach (JsonElement rule in ruleArray.EnumerateArray())
            {
                string id = GetString(rule, "id", string.Empty);
                rules.Ids.Add(string.IsNullOrWhiteSpace(id) ? "(no rule)" : id);
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                string description = GetMessageText(rule, "shortDescription");
                if (string.IsNullOrWhiteSpace(description))
                {
                    description = GetMessageText(rule, "fullDescription");
                }

                string helpUri = GetString(rule, "helpUri", string.Empty);
                string metadata = TryGetProperty(rule, "properties", out JsonElement properties)
                    ? JsonSerializer.Serialize(properties, new JsonSerializerOptions { WriteIndented = true })
                    : string.Empty;
                rules.ById[id] = new RuleInfo(description, helpUri, metadata);
            }
            return rules;
        }

        private static FindingRow ReadFinding(JsonElement result, RuleCatalog rules)
        {
            string ruleId = GetString(result, "ruleId", string.Empty);
            if (string.IsNullOrWhiteSpace(ruleId)
                && TryGetProperty(result, "ruleIndex", out JsonElement ruleIndex)
                && ruleIndex.ValueKind == JsonValueKind.Number
                && ruleIndex.TryGetInt32(out int parsedRuleIndex)
                && parsedRuleIndex >= 0
                && parsedRuleIndex < rules.Ids.Count)
            {
                ruleId = rules.Ids[parsedRuleIndex];
            }
            if (string.IsNullOrWhiteSpace(ruleId))
            {
                ruleId = "(no rule)";
            }
            string level = NormalizeLevel(GetString(result, "level", "warning"));
            string message = GetMessageText(result, "message");
            string file = string.Empty;
            int line = 0;

            if (TryGetArray(result, "locations", out JsonElement locations))
            {
                JsonElement.ArrayEnumerator locationEnumerator = locations.EnumerateArray();
                if (locationEnumerator.MoveNext())
                {
                    JsonElement location = locationEnumerator.Current;
                    if (TryGetProperty(location, "physicalLocation", out JsonElement physicalLocation))
                    {
                        if (TryGetProperty(physicalLocation, "artifactLocation", out JsonElement artifactLocation))
                        {
                            file = GetString(artifactLocation, "uri", string.Empty);
                        }
                        if (TryGetProperty(physicalLocation, "region", out JsonElement region)
                            && TryGetProperty(region, "startLine", out JsonElement startLine)
                            && startLine.ValueKind == JsonValueKind.Number
                            && startLine.TryGetInt32(out int parsedLine))
                        {
                            line = parsedLine;
                        }
                    }
                }
            }

            RuleInfo ruleInfo = rules.ById.TryGetValue(ruleId, out RuleInfo? foundRule)
                ? foundRule
                : RuleInfo.Empty;
            return new FindingRow(ruleId, level, message, file, line, ruleInfo.Description, ruleInfo.HelpUri, ruleInfo.Metadata);
        }

        private static void ReadInvocationTime(JsonElement run, ref DateTimeOffset? latestTime)
        {
            if (!TryGetArray(run, "invocations", out JsonElement invocations))
            {
                return;
            }

            foreach (JsonElement invocation in invocations.EnumerateArray())
            {
                string endTime = GetString(invocation, "endTime", string.Empty);
                if (DateTimeOffset.TryParse(endTime, out DateTimeOffset parsedTime)
                    && (!latestTime.HasValue || parsedTime > latestTime.Value))
                {
                    latestTime = parsedTime;
                }
            }
        }

        private static string GetMessageText(JsonElement parent, string propertyName)
        {
            if (!TryGetProperty(parent, propertyName, out JsonElement message))
            {
                return string.Empty;
            }

            return GetString(message, "text", GetString(message, "markdown", string.Empty));
        }

        private static string GetString(JsonElement parent, string propertyName, string defaultValue)
        {
            if (TryGetProperty(parent, propertyName, out JsonElement value)
                && value.ValueKind == JsonValueKind.String)
            {
                return value.GetString() ?? defaultValue;
            }
            return defaultValue;
        }

        private static bool TryGetProperty(JsonElement parent, string propertyName, out JsonElement value)
        {
            if (parent.ValueKind == JsonValueKind.Object)
            {
                return parent.TryGetProperty(propertyName, out value);
            }
            value = default;
            return false;
        }

        private static bool TryGetArray(JsonElement parent, string propertyName, out JsonElement value)
        {
            if (TryGetProperty(parent, propertyName, out value) && value.ValueKind == JsonValueKind.Array)
            {
                return true;
            }
            value = default;
            return false;
        }

        private static string NormalizeLevel(string level)
        {
            if (string.Equals(level, "error", StringComparison.OrdinalIgnoreCase))
            {
                return "Error";
            }
            if (string.Equals(level, "warning", StringComparison.OrdinalIgnoreCase))
            {
                return "Warning";
            }
            if (string.Equals(level, "note", StringComparison.OrdinalIgnoreCase))
            {
                return "Note";
            }
            return "Other";
        }

        private void UpdateSummary(List<FindingRow> findings)
        {
            mTotalSummaryButton.Text = string.Format("Total    {0:N0}", findings.Count);
            mErrorSummaryButton.Text = string.Format("Errors    {0:N0}", CountLevel(findings, "Error"));
            mWarningSummaryButton.Text = string.Format("Warnings    {0:N0}", CountLevel(findings, "Warning"));
            mNoteSummaryButton.Text = string.Format("Notes    {0:N0}", CountLevel(findings, "Note"));
            mOtherSummaryButton.Text = string.Format("Other    {0:N0}", CountLevel(findings, "Other"));
        }

        private static int CountLevel(List<FindingRow> findings, string level)
        {
            return findings.Count(finding => finding.Level == level);
        }

        private void UpdateTopRules(Dictionary<string, int> ruleCounts)
        {
            mTopRulesListView.Items.Clear();
            foreach (KeyValuePair<string, int> rule in ruleCounts
                .OrderByDescending(entry => entry.Value)
                .ThenBy(entry => entry.Key, StringComparer.Ordinal)
                .Take(12))
            {
                ListViewItem item = new ListViewItem(rule.Key);
                item.SubItems.Add(rule.Value.ToString("N0"));
                mTopRulesListView.Items.Add(item);
            }
        }

        private void UpdateDistribution(List<FindingRow> findings)
        {
            mSeverityChart.SetCounts(
                CountLevel(findings, "Error"),
                CountLevel(findings, "Warning"),
                CountLevel(findings, "Note"),
                CountLevel(findings, "Other"));
        }

        private void ApplyFilters()
        {
            string query = mSearchTextBox.Text.Trim();
            string severity = mSeverityComboBox.SelectedItem as string ?? ALL_SEVERITIES;
            List<FindingRow> visibleFindings = mFindings
                .Where(finding => severity == ALL_SEVERITIES || finding.Level == severity)
                .Where(finding => string.IsNullOrEmpty(mActiveRuleFilter)
                    || finding.RuleId == mActiveRuleFilter)
                .Where(finding => string.IsNullOrEmpty(query)
                    || finding.RuleId.Contains(query, StringComparison.OrdinalIgnoreCase)
                    || finding.Message.Contains(query, StringComparison.OrdinalIgnoreCase)
                    || finding.File.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            mResultsGrid.Rows.Clear();
            foreach (FindingRow finding in visibleFindings)
            {
                int rowIndex = mResultsGrid.Rows.Add(
                    finding.Level,
                    finding.RuleId,
                    finding.Message,
                    finding.File,
                    finding.Line > 0 ? finding.Line.ToString() : string.Empty);
                mResultsGrid.Rows[rowIndex].Tag = finding;
            }
            mStatusLabel.Text = string.Format(
                "Showing {0:N0} of {1:N0} findings{2}",
                visibleFindings.Count,
                mFindings.Count,
                string.IsNullOrEmpty(mActiveRuleFilter) ? string.Empty : "  •  Rule: " + mActiveRuleFilter);
            if (mResultsGrid.Rows.Count > 0)
            {
                mResultsGrid.Rows[0].Selected = true;
                ShowFindingDetails(mResultsGrid.Rows[0].Tag as FindingRow);
            }
            else
            {
                ShowFindingDetails(null);
            }
        }

        private void ShowFindingDetails(FindingRow? finding)
        {
            if (finding == null)
            {
                mDetailsTextBox.Text = "No finding selected.";
                mMetadataTextBox.Text = string.Empty;
                mMetadataToggleButton.Enabled = false;
                mMetadataPanel.Visible = false;
                mMetadataToggleButton.Text = "Show rule metadata";
                return;
            }

            StringBuilder details = new StringBuilder();
            details.AppendLine(finding.Level + "  ·  " + finding.RuleId);
            details.AppendLine();
            details.AppendLine(string.IsNullOrWhiteSpace(finding.Message) ? "(No message)" : finding.Message);
            details.AppendLine();
            details.Append("Location: ");
            details.Append(string.IsNullOrWhiteSpace(finding.File) ? "(not provided)" : finding.File);
            if (finding.Line > 0)
            {
                details.Append(":");
                details.Append(finding.Line);
            }
            mDetailsTextBox.Text = details.ToString();

            StringBuilder metadata = new StringBuilder();
            metadata.AppendLine("Rule: " + finding.RuleId);
            if (!string.IsNullOrWhiteSpace(finding.RuleDescription))
            {
                metadata.AppendLine(finding.RuleDescription);
            }
            if (!string.IsNullOrWhiteSpace(finding.HelpUri))
            {
                metadata.AppendLine("Help: " + finding.HelpUri);
            }
            if (!string.IsNullOrWhiteSpace(finding.RuleMetadata))
            {
                metadata.AppendLine(finding.RuleMetadata);
            }
            mMetadataTextBox.Text = metadata.ToString();
            mMetadataToggleButton.Enabled =
                !string.IsNullOrWhiteSpace(finding.RuleDescription)
                || !string.IsNullOrWhiteSpace(finding.HelpUri)
                || !string.IsNullOrWhiteSpace(finding.RuleMetadata);
        }

        private void ShowEmptyState()
        {
            List<FindingRow> noFindings = new List<FindingRow>();
            UpdateSummary(noFindings);
            UpdateDistribution(noFindings);
            mTopRulesListView.Items.Clear();
            mResultsGrid.Rows.Clear();
            ShowFindingDetails(null);
            mStatusLabel.Text = "No SARIF report loaded.";
        }

        private void searchTextBox_TextChanged(object? sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void severityComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void clearFiltersButton_Click(object? sender, EventArgs e)
        {
            mActiveRuleFilter = string.Empty;
            mSearchTextBox.Clear();
            mSeverityComboBox.SelectedIndex = 0;
            mTopRulesListView.SelectedItems.Clear();
            ApplyFilters();
        }

        private void summaryCard_Click(object? sender, EventArgs e)
        {
            if (sender is Button card && card.Tag is string severity && !string.IsNullOrEmpty(severity))
            {
                mSeverityComboBox.SelectedItem = severity;
            }
            else
            {
                mSeverityComboBox.SelectedIndex = 0;
            }
        }

        private void topRulesListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            mActiveRuleFilter = mTopRulesListView.SelectedItems.Count == 0
                ? string.Empty
                : mTopRulesListView.SelectedItems[0].Text;
            ApplyFilters();
        }

        private void resultsGrid_SelectionChanged(object? sender, EventArgs e)
        {
            if (mResultsGrid.SelectedRows.Count == 0)
            {
                return;
            }
            ShowFindingDetails(mResultsGrid.SelectedRows[0].Tag as FindingRow);
        }

        private void metadataToggleButton_Click(object? sender, EventArgs e)
        {
            mMetadataPanel.Visible = !mMetadataPanel.Visible;
            mMetadataToggleButton.Text = mMetadataPanel.Visible
                ? "Hide rule metadata"
                : "Show rule metadata";
        }

        private void distributionChart_SeveritySelected(string severity)
        {
            mSeverityComboBox.SelectedItem = severity;
        }

        private sealed class FindingRow
        {
            public string RuleId { get; }
            public string Level { get; }
            public string Message { get; }
            public string File { get; }
            public int Line { get; }
            public string RuleDescription { get; }
            public string HelpUri { get; }
            public string RuleMetadata { get; }

            public FindingRow(
                string ruleId,
                string level,
                string message,
                string file,
                int line,
                string ruleDescription,
                string helpUri,
                string ruleMetadata)
            {
                RuleId = ruleId;
                Level = level;
                Message = message;
                File = file;
                Line = line;
                RuleDescription = ruleDescription;
                HelpUri = helpUri;
                RuleMetadata = ruleMetadata;
            }
        }

        private sealed class RuleInfo
        {
            public static RuleInfo Empty { get; } = new RuleInfo(string.Empty, string.Empty, string.Empty);

            public string Description { get; }
            public string HelpUri { get; }
            public string Metadata { get; }

            public RuleInfo(string description, string helpUri, string metadata)
            {
                Description = description;
                HelpUri = helpUri;
                Metadata = metadata;
            }
        }

        private sealed class RuleCatalog
        {
            public Dictionary<string, RuleInfo> ById { get; }
            public List<string> Ids { get; }

            public RuleCatalog()
            {
                ById = new Dictionary<string, RuleInfo>(StringComparer.Ordinal);
                Ids = new List<string>();
            }
        }

        private sealed class SeverityBarChart : Control
        {
            private readonly string[] mSeverities = { "Error", "Warning", "Note", "Other" };
            private readonly Color[] mColors =
            {
                Color.FromArgb(207, 34, 46),
                Color.FromArgb(191, 135, 0),
                Color.FromArgb(9, 105, 218),
                Color.FromArgb(139, 148, 158)
            };
            private readonly int[] mCounts = new int[4];

            public event Action<string>? SeveritySelected;

            public SeverityBarChart()
            {
                DoubleBuffered = true;
                BackColor = Color.White;
                Cursor = Cursors.Hand;
                MinimumSize = new Size(200, 30);
            }

            public void SetCounts(int errors, int warnings, int notes, int other)
            {
                mCounts[0] = errors;
                mCounts[1] = warnings;
                mCounts[2] = notes;
                mCounts[3] = other;
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs paintEvent)
            {
                base.OnPaint(paintEvent);
                Graphics graphics = paintEvent.Graphics;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                int total = mCounts.Sum();
                int barTop = 6;
                int barHeight = 16;
                int barWidth = Math.Max(0, ClientSize.Width - 2);
                int currentX = 1;

                using (SolidBrush backgroundBrush = new SolidBrush(Color.FromArgb(234, 238, 242)))
                {
                    graphics.FillRectangle(backgroundBrush, 1, barTop, barWidth, barHeight);
                }

                if (total > 0)
                {
                    int cumulativeCount = 0;
                    for (int index = 0; index < mCounts.Length; index++)
                    {
                        cumulativeCount += mCounts[index];
                        int nextX = (int)Math.Round(barWidth * (double)cumulativeCount / total);
                        int segmentWidth = nextX - currentX;
                        if (mCounts[index] > 0 && segmentWidth > 0)
                        {
                            using (SolidBrush severityBrush = new SolidBrush(mColors[index]))
                            {
                                graphics.FillRectangle(severityBrush, currentX, barTop, segmentWidth, barHeight);
                            }
                        }
                        currentX = nextX;
                    }
                }

                int legendX = 2;
                int legendY = 31;
                using (Font legendFont = new Font(Font.FontFamily, 8F))
                using (Brush textBrush = new SolidBrush(ForeColor))
                {
                    for (int index = 0; index < mSeverities.Length; index++)
                    {
                        using (SolidBrush severityBrush = new SolidBrush(mColors[index]))
                        {
                            graphics.FillRectangle(severityBrush, legendX, legendY + 3, 8, 8);
                        }
                        string legend = string.Format("{0} {1:N0}", mSeverities[index], mCounts[index]);
                        graphics.DrawString(legend, legendFont, textBrush, legendX + 12, legendY);
                        legendX += (int)graphics.MeasureString(legend, legendFont).Width + 26;
                    }
                }
            }

            protected override void OnMouseClick(MouseEventArgs mouseEvent)
            {
                base.OnMouseClick(mouseEvent);
                if (mouseEvent.Y < 4 || mouseEvent.Y > 26)
                {
                    return;
                }

                int total = mCounts.Sum();
                if (total == 0)
                {
                    return;
                }

                int position = mouseEvent.X - 1;
                int width = Math.Max(1, ClientSize.Width - 2);
                int accumulatedWidth = 0;
                for (int index = 0; index < mCounts.Length; index++)
                {
                    if (mCounts[index] == 0)
                    {
                        continue;
                    }
                    accumulatedWidth += (int)Math.Round(width * (double)mCounts[index] / total);
                    if (position <= accumulatedWidth)
                    {
                        SeveritySelected?.Invoke(mSeverities[index]);
                        return;
                    }
                }
            }
        }
    }
}
