using System.Diagnostics;
using DuckChatForm.DllRapper;
using DuckChatForm.DuckChatForm.CustomControl;
using DuckChatForm.DuckChatForm.Popup;
using DuckChatForm.Models;

namespace DuckChatForm
{
    public struct LoginUserInfo
    {
        public User LoginUser;
        public bool IsLogin;

        public LoginUserInfo(string name)
        {
            LoginUser = new User(name);
            IsLogin = true;
        }
    }
    public struct ChatMessage
    {
        public User Sender;
        public string Message;
        public DateTime Time;
    }

    public partial class MainForm : Form
    {
        #region 폼 속성
        public const int MIN_WIDTH = 900;
        public const int MIN_HEIGHT = 600;
        #endregion

        #region 통신 속성
        private NativeDuckClient? mClientOrNull;
        private LoginUserInfo? mLoginUserOrNull;
        private readonly List<ChatMessage> mChatMessageList = new List<ChatMessage>(4096);
        public const string DEFAULT_ADDRESS = "localhost";
        public const string DEFAULT_PORT = "27015";
        #endregion

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            using (LoginForm loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog(this) != DialogResult.OK)
                {
                    MessageBox.Show("로그인 없이 진행 합니다.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    desableSendUI();
                    return;
                }

                mLoginUserOrNull = new LoginUserInfo(loginForm.UserName);
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
                desableSendUI();
            }
        }

        private void disposeClient()
        {
            if (mClientOrNull != null)
            {
                mClientOrNull.Dispose();
                mClientOrNull = null;
            }
        }

        #region MenuItem Event
        private void changeLoginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (LoginForm loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog(this) == DialogResult.OK)
                {
                    mLoginUserOrNull = new LoginUserInfo(loginForm.UserName);
                    if (TryConnectServer())
                    {
                        enableSendUI();
                    }
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
        #endregion

        private bool TryConnectServer()
        {
            try
            {
                mClientOrNull = new NativeDuckClient(DEFAULT_ADDRESS, DEFAULT_PORT);
                return true;
            }
            catch (Exception exception) when (exception is InvalidOperationException
                || exception is DllNotFoundException
                || exception is EntryPointNotFoundException
                || exception is BadImageFormatException)
            {
                MessageBox.Show(this, exception.Message, "서버 연결 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                desableSendUI();
                return false;
            }
        }

        private void desableSendUI()
        {
            setSendUIEnableState(false);
        }

        private void enableSendUI()
        {
            setSendUIEnableState(true);
        }

        private void setSendUIEnableState(bool enable)
        {
            textBoxMessage.Enabled = enable;
            buttonSend.Enabled = enable;
            buttonAddFile.Enabled = enable;
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            Debug.Assert(mLoginUserOrNull != null);
            Debug.Assert(mClientOrNull != null && mClientOrNull.IsConnected);

            string message = textBoxMessage.Text.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (!mClientOrNull!.SendChat(message))
            {
                MessageBox.Show(this, "메시지를 전송하지 못했습니다.", "전송 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            listBoxMessages.Items.Add("나: " + message);
            listBoxMessages.TopIndex = listBoxMessages.Items.Count - 1;
            textBoxMessage.Clear();
            textBoxMessage.Focus();

            mChatMessageList.Add(new ChatMessage()
            {
                Sender = mLoginUserOrNull!.Value.LoginUser,
                Message = message,
                Time = DateTime.Now
            });
        }

        private void textBoxMessage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonSend.PerformClick();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
    }
}
