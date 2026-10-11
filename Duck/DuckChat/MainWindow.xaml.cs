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
        public const int MIN_WIDTH = 900;
        public const int MIN_HEIGHT = 600;

        public MainWindow()
        {
            InitializeComponent();

            AppWindow.Resize(new SizeInt32(MIN_WIDTH, MIN_HEIGHT));
            SetPresenter();
        }

        private void SetPresenter()
        {
            OverlappedPresenter presenter = OverlappedPresenter.Create();
            presenter.PreferredMinimumWidth = MIN_WIDTH;
            presenter.PreferredMinimumHeight = MIN_HEIGHT;

            AppWindow.SetPresenter(presenter);
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
        }
    }
}
