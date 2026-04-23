using DuckPhysics.Physics2D;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WatermelonGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitGame();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            InitWorld2D();
            ClearCanvas();
            UpdateCanvasObjects();
        }

        private void MainCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(mainRenderngCanvas);

            float x = (float)pos.X;
            float y = (float)pos.Y;

            float dropHeight = 500f;

            AddNewFuruit(new System.Numerics.Vector2(x, dropHeight), EWatermelonLevel.ZERO);
        }
    }
}
