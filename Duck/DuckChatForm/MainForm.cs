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
        private NativeDuckClient? mClientOrNull;
        private UserInfo? mLoginUserOrNull;
        public const string DEFAULT_ADDRESS = "localhost";
        public const string DEFAULT_PORT = "27015";
        #endregion

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            using (LoginForm loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog(this) != DialogResult.OK)
                {
                    MessageBox.Show("로그인을 해야만 사용할 수 있습니다.", "Error");
                    Close();
                    return;
                }

                mLoginUserOrNull = new UserInfo(loginForm.UserName, "");
            }

            try
            {
                mClientOrNull = new NativeDuckClient(DEFAULT_ADDRESS, DEFAULT_PORT);
            }
            catch (Exception exception) when (exception is InvalidOperationException
                || exception is DllNotFoundException
                || exception is EntryPointNotFoundException
                || exception is BadImageFormatException)
            {
                MessageBox.Show(this, exception.Message, "서버 연결 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void changeLoginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (LoginForm loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog(this) == DialogResult.OK)
                {
                    mLoginUserOrNull = new UserInfo(loginForm.UserName, "");
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutBox aboutBox = new AboutBox())
            {
                aboutBox.ShowDialog(this);
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            string message = textBoxMessage.Text.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (mClientOrNull == null)
            {
                MessageBox.Show(this, "서버에 연결되어 있지 않습니다.", "전송 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (!mClientOrNull.SendChat(message))
                {
                    MessageBox.Show(this, "메시지를 전송하지 못했습니다.", "전송 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (ArgumentException exception)
            {
                MessageBox.Show(this, exception.Message, "전송 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            listBoxMessages.Items.Add("나: " + message);
            listBoxMessages.TopIndex = listBoxMessages.Items.Count - 1;
            textBoxMessage.Clear();
            textBoxMessage.Focus();
        }

        private void disposeClient()
        {
            if (mClientOrNull != null)
            {
                mClientOrNull.Dispose();
                mClientOrNull = null;
            }
        }
    }
}
