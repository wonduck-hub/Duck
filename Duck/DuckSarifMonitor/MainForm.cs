using DuckSarifMonitor.SarifHandling;

namespace DuckSarifMonitor
{
    internal partial class MainForm : Form
    {
        public DuckSarifLog? SarifLogOrNull { get; private set; }
        public string SarifLogPath = string.Empty;

        public MainForm()
        {
            InitializeComponent();
        }

        private void loadSARIFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Sarif files (*.sarif)|*.sarif";
            ofd.Multiselect = false;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    DuckSarifLog loadedLog = new DuckSarifLog(ofd.FileName, ESarifProjectID.NONE);
                    mSarifDashboardView.LoadSarif(ofd.FileName);
                    SarifLogOrNull = loadedLog;
                    SarifLogPath = ofd.FileName;
                    initSingleView();
                    toolStripStatusLabelStatus.Text = "Loaded " + Path.GetFileName(ofd.FileName);
                }
                catch (Exception exception) // sarif 파일이 2.1.0이 아닌 경우 
                {
                    MessageBox.Show(
                        this,
                        "Could not load the SARIF report.\r\n" + exception.Message,
                        "Load SARIF",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    toolStripStatusLabelStatus.Text = "SARIF load failed";
                }
            }
        }

        private void initSingleView()
        {
            if (SarifLogOrNull == null)
            {
                return;
            }

            toolStripTextBoxSarifFilePath.Text = SarifLogPath;
            toolStripTextBoxSarifFilePath.ToolTipText = SarifLogPath;
        }

        private void buttonSingleDashboard_Click(object sender, EventArgs e)
        {
            panelSingleDashboard.BringToFront();
        }

        private void buttonQualityTrends_Click(object sender, EventArgs e)
        {
            panelSingleDashboard.BringToFront();
        }
    }
}
