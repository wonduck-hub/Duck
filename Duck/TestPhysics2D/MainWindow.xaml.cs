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

namespace TestPhysics2D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private World2D mWorld;
        private DispatcherTimer mTimer;

        // 물리 객체 ↔ UI 매핑
        private Dictionary<DinamicObject2D, Shape> renderObjects = new();

        public MainWindow()
        {
            InitializeComponent();

            mTimer = new DispatcherTimer();

            InitWorld2D();
            ShowObjectsInCanvas();

            // 타이머 (렌더 루프)
            mTimer.Interval = TimeSpan.FromMilliseconds(16);
            mTimer.Tick += Update;
            mTimer.Start();
        }

        private void InitWorld2D()
        {
            if (mWorld != null)
            {
                mWorld.Dispose();
            }

            mWorld = new World2D();

            mWorld.Width = 600;
            mWorld.Height = 400;
            mWorld.HasGravity = true;
            mWorld.Gravity = 0.5f;

            mWorld.Start();
        }

        private void ShowObjectsInCanvas()
        {
            // add objects in Canvas
            foreach (var obj in mWorld.Objects)
            {
                if (obj is DinamicObject2D dyn)
                {
                    Shape shape;

                    if (dyn.Collider.Type == EColliderType.CIRCLE)
                    {
                        shape = new Ellipse()
                        {
                            Width = dyn.Collider.Size.X,
                            Height = dyn.Collider.Size.Y,
                            Fill = Brushes.Red
                        };
                    }
                    else
                    {
                        shape = new Rectangle()
                        {
                            Width = dyn.Collider.Size.X,
                            Height = dyn.Collider.Size.Y,
                            Fill = Brushes.Blue
                        };
                    }

                    MainCanvas.Children.Add(shape);
                    renderObjects[dyn] = shape;
                }
            }
        }

        private void ClearCanvas()
        {
            MainCanvas.Children.Clear();
            renderObjects.Clear();
        }

        // Tic Event
        private void Update(object? sender, EventArgs e)
        {
            mWorld.Update();

            foreach (var pair in renderObjects)
            {
                var obj = pair.Key;
                var shape = pair.Value;

                // 중심 기준 → 좌상단 보정
                double x = obj.Position.X - shape.Width / 2;
                double y = (mWorld.Height - obj.Position.Y) - shape.Height / 2;

                Canvas.SetLeft(shape, x);
                Canvas.SetTop(shape, y);
            }
        }

        #region UI Event
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            InitWorld2D();
            ClearCanvas();
            ShowObjectsInCanvas();
        }

        #endregion
    }
}
