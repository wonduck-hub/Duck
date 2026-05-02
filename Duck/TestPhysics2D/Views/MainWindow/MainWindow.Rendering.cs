using DuckPhysics.Physics2D;
using System.Diagnostics;
using System.IO;
using System.Numerics;
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

        private GameWorld2D mWorld = null;
        private DispatcherTimer mTimer = null;
        private Dictionary<EWatermelonLevel, ImageBrush> mWatermelonLevelToImage = new();

        // 물리 객체 ↔ UI 매핑
        private Dictionary<DinamicObject2D, Shape> renderObjects = new();

        private void InitGame()
        {
            mTimer = new DispatcherTimer();

            InitWorld2D();
            UpdateCanvasObjects();

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

            mWorld = new GameWorld2D();

            mWorld.Width = 600;
            mWorld.Height = 400;
            mWorld.HasGravity = true;
            mWorld.Gravity = 9.8f;

            mWorld.Start();

            mWorld.Objects.Clear();
        }

        #region rendering
        private void UpdateCanvasObjects()
        {
            ClearCanvas();

            // add objects in Canvas
            foreach (var obj in mWorld.Objects)
            {
                if (obj is WatermelonObject wObj)
                {
                    Shape shape;

                    if (wObj.Collider.Type == EColliderType.CIRCLE)
                    {
                        shape = new Ellipse()
                        {
                            Width = wObj.Collider.Size.X,
                            Height = wObj.Collider.Size.Y,
                        };

                        //Debug.Assert(mWatermelonLevelToImage.ContainsKey(wObj.Level), "물체 레벨에 따른 이미지가 매핑되어 있어야 합니다.");

                        if (mWatermelonLevelToImage.ContainsKey(wObj.Level))
                        {
                            shape.Fill = mWatermelonLevelToImage[wObj.Level];
                        }
                        else
                        {
                            Debug.WriteLine("현재 매핑되는 이미지가 없음.");
                            shape.Fill = new SolidColorBrush(Colors.Red);
                        }
                    }
                    else
                    {
                        Debug.Fail("현재는 Circle 타입의 2D 객체만 렌더링이 구현되어 있습니다.");
                        shape = null;
                    }

                    mainRenderngCanvas.Children.Add(shape);
                    renderObjects[wObj] = shape;
                }
            }

            // 개체들을 렌더링 할 캔버스 좌표에 배치
            foreach (var pair in renderObjects)
            {
                var obj = pair.Key;
                var shape = pair.Value;

                // 중심 기준 → 좌상단 보정
                double x = obj.Position.X - shape.Width / 2;
                double y = (mWorld.Height - obj.Position.Y) - shape.Height / 2;

                // 회전 렌더링 적용
                shape.RenderTransformOrigin = new Point(0.5, 0.5);
                shape.RenderTransform = new RotateTransform(obj.Rotation * 180.0 / Math.PI);

                Canvas.SetLeft(shape, x);
                Canvas.SetTop(shape, y);
            }
        }

        private void ClearCanvas()
        {
            mainRenderngCanvas.Children.Clear();
            renderObjects.Clear();
        }

        private bool AddNewFuruit(Vector2 position, EWatermelonLevel level)
        {
            if (mWorld.TryAddObject(new WatermelonObject(position, level)))
            {
                return true;
            }
            return false;
        }

        // Tic Event
        private void Update(object? sender, EventArgs e)
        {
            mWorld.Update();

            UpdateCanvasObjects(); // 월드 업데이트에 따른 개체 표시 여부를 판단하고 갱신하는 함수


        }

        private void SetWatermellonImage(string imageFolderpath)
        {
            DirectoryInfo imgDirPath = new DirectoryInfo(imageFolderpath);
            if (!Directory.Exists(imgDirPath.FullName))
            {
                MessageBox.Show("해당 경로가 존재하지 않습니다", "경로 오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // 이미지 파일 확장자 필터링
            string[] extensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

            FileInfo[] imageFiles = imgDirPath.GetFiles()
                                              .Where(f => extensions.Contains(f.Extension.ToLower()))
                                              .ToArray();

            for (int i = 0; i < Enum.GetValues(typeof(EWatermelonLevel)).Length; ++i)
            {
                string? targetImagePathOrNull = null;
                foreach (FileInfo img in imageFiles)
                {
                    if (System.IO.Path.GetFileNameWithoutExtension(img.Name) == (i + 1).ToString())
                    {
                        // 확장자가 이미지인지 확인
                        foreach (string ext in extensions)
                        {
                            if (img.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase))
                            {
                                targetImagePathOrNull = img.FullName;
                                break;
                            }
                        }
                    }
                }

                if (targetImagePathOrNull == null)
                {
                    continue;
                }

                ImageBrush imgBrush = new ImageBrush();
                imgBrush.ImageSource = new BitmapImage(new Uri(targetImagePathOrNull, UriKind.Absolute));

                mWatermelonLevelToImage.Add((EWatermelonLevel)Enum.GetValues(typeof(EWatermelonLevel)).GetValue(i), imgBrush);
            }
        }
        #endregion
    }
}
