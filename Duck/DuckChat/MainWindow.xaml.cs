using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DuckChat.DllRapper;
using DuckChat.DuckChat.User;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Graphics;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DuckChat
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
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

        public MainWindow()
        {
            InitializeComponent();

            mClient = new NativeDuckClient(DEFAULT_ADDRESS, DEFAULT_PORT);

            AppWindow.Resize(new SizeInt32(MIN_WIDTH * 2, MIN_HEIGHT * 2));
            setPresenter();

            RootGrid.Loaded += rootGrid_Loaded;
        }

        private void setPresenter()
        {
            OverlappedPresenter presenter = OverlappedPresenter.Create();
            presenter.PreferredMinimumWidth = MIN_WIDTH;
            presenter.PreferredMinimumHeight = MIN_HEIGHT;

            AppWindow.SetPresenter(presenter);
        }

        private async Task<bool> login()
        {
            TextBox nameTextBox = new TextBox();
            nameTextBox.PlaceholderText = "닉네임을 입력하세요";

            ContentDialog dialog = new ContentDialog();
            dialog.Title = "사용자 정보";
            dialog.Content = nameTextBox;
            dialog.PrimaryButtonText = "확인";
            dialog.CloseButtonText = "취소";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.XamlRoot = RootGrid.XamlRoot;

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                mLoginUser = new UserInfo(nameTextBox.Text, "");
                return true;
            }

            return false;
        }

        private async void rootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            RootGrid.Loaded -= rootGrid_Loaded;

            bool bLoggedIn = await login();

            if (!bLoggedIn)
            {
                Close();
            }
        }

        private void serverNavigationView_SelectionChanged(
            NavigationView sender,
            NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is not NavigationViewItem selectedItem)
            {
                return;
            }

            if (selectedItem.Tag is string selectedServerName)
            {
                SelectedServerTitle.Text = selectedServerName;
            }
        }

        private void sendButton_Click(object sender, RoutedEventArgs args)
        {
            sendMessage();
        }

        private void messageInput_KeyDown(object sender, KeyRoutedEventArgs args)
        {
            if (args.Key == VirtualKey.Enter)
            {
                sendMessage();
                args.Handled = true;
            }
        }

        private void sendMessage()
        {
            Debug.Assert(mClient.Handle != IntPtr.Zero);

            string message = MessageInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            StackPanel messageContainer = new StackPanel();
            messageContainer.Spacing = 4;

            TextBlock senderLabel = new TextBlock();
            senderLabel.Text = "나";
            senderLabel.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold;

            TextBlock messageText = new TextBlock();
            messageText.Text = message;
            messageText.TextWrapping = TextWrapping.Wrap;

            messageContainer.Children.Add(senderLabel);
            messageContainer.Children.Add(messageText);
            MessageList.Children.Add(messageContainer);

            EmptyMessageHint.Visibility = Visibility.Collapsed;
            MessageInput.Text = string.Empty;

            MessageScrollViewer.UpdateLayout();
            MessageScrollViewer.ChangeView(null, MessageScrollViewer.ExtentHeight, null);
            MessageInput.Focus(FocusState.Programmatic);

            mClient.SendChat(message);
        }
    }
}
