

namespace DuckChatForm.DuckChatForm.Popup
{
    public partial class LoginForm : Form
    {
        public string UserName
        {
            get
            {
                return textBoxName.Text;
            }

            private set;
        }
        public LoginForm()
        {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
