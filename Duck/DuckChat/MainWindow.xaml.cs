using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;

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
    }
}
