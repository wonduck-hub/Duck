using DuckChat.DllRapper;
using DuckChat.DuckChat.User;
using DuckChatForm.DuckChatForm.CustomControl;
using DuckChatForm.DuckChatForm.Popup;

namespace DuckChatForm
{
    public partial class MainForm : Form
    {
        #region 폼 속성
        public const int MIN_WIDTH = 900;
        public const int MIN_HEIGHT = 600;
        #endregion

        #region 통신 속성
        private readonly NativeDuckClient mClient;
        private UserInfo mLoginUser;
        public const string DEFAULT_ADDRESS = "localhost";
        public const string DEFAULT_PORT = "27015";
        #endregion

        public MainForm()
        {
            InitializeComponent();

            mClient = new NativeDuckClient(DEFAULT_ADDRESS, DEFAULT_PORT);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoginForm lf = new LoginForm();
            if (lf.ShowDialog(this) == DialogResult.OK)
            {
                mLoginUser = new UserInfo(lf.UserName, "");
            }
            else
            {
                MessageBox.Show("로그인을 해야만 사용할 수 있습니다.", "Error");
                this.Close();
            }

            
            mClient.IsConnected();
        }

        private void changeLoginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoginForm lf = new LoginForm();
            if (lf.ShowDialog(this) == DialogResult.OK)
            {
                mLoginUser = new UserInfo(lf.UserName, "");
            }

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox ab = new AboutBox();
            ab.ShowDialog(this);
        }
    }
}
