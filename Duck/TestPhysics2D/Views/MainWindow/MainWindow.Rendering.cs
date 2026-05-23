using DuckPhysics.Physics2D;
using System.Diagnostics;
using System.Drawing;
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
        private Dictionary<EWatermelonLevel, LevelVisual?> mWatermelonLevelToVisual = new();

        // 물리 객체 ↔ UI 매핑
        private Dictionary<IObject2D, Shape> renderObjects = new();

        private void InitGame()
        {
            mTimer = new DispatcherTimer();

            InitWorld2D();
            UpdateCanvasObjects();

            // 타이머 (렌더 루프)
            mTimer.Interval = TimeSpan.FromMilliseconds(16);
            mTimer.Tick += Update;
            mTimer.Start();

            #region
            foreach (EWatermelonLevel level in Enum.GetValues(typeof(EWatermelonLevel)))
            {
                LevelVisual visual = null;
                switch (level)
                {
                    case EWatermelonLevel.ZERO:
                        visual = new LevelVisual(null, new Vector2(20, 20));
                        break;
                    case EWatermelonLevel.ONE:
                        visual = new LevelVisual(null, new Vector2(25, 25));
                        break;
                    case EWatermelonLevel.TWO:
                        visual = new LevelVisual(null, new Vector2(30, 30));
                        break;
                    case EWatermelonLevel.THREE:
                        visual = new LevelVisual(null, new Vector2(35, 35));
                        break;
                    case EWatermelonLevel.FOUR:
                        visual = new LevelVisual(null, new Vector2(40, 40));
                        break;
                    case EWatermelonLevel.FIVE:
                        visual = new LevelVisual(null, new Vector2(50, 50));
                        break;
                    case EWatermelonLevel.SIX:
                        visual = new LevelVisual(null, new Vector2(60, 60));
                        break;
                    case EWatermelonLevel.SEVEN:
                        visual = new LevelVisual(null, new Vector2(70, 70));
                        break;
                    case EWatermelonLevel.EIGHT:
                        visual = new LevelVisual(null, new Vector2(80, 80));
                        break;
                    case EWatermelonLevel.NINE:
                        visual = new LevelVisual(null, new Vector2(90, 90));
                        break;
                    case EWatermelonLevel.TEN:
                        visual = new LevelVisual(null, new Vector2(100, 100));
                        break;
                    case EWatermelonLevel.ELEVEN:
                        visual = new LevelVisual(null, new Vector2(120, 120));
                        break;
                    default:
                        Debug.Fail("지원하지 않는 WatermelonLevel 입니다.");
                        break;
                }
                mWatermelonLevelToVisual.Add(level, visual);
            }
            #endregion

            Debug.Assert(mWatermelonLevelToVisual.Count == Enum.GetValues(typeof(EWatermelonLevel)).Length, "시작 시 모든 레벨에 대한 이미지가 매핑되어 있어야 합니다.");
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

            mWorld.Objects.Clear();
            mWorld.Start();

            Basket basket = new Basket(
                new Vector2(200, 200),
                new Vector2(100, 200),
                10f);

            foreach (StaticObject2D wall in basket.Walls)
            {
                mWorld.TryAddObject(wall);
            }
        }

        public class Basket // 바구니 용 중첩 클래스, 월드에 추가되는 정적 개체들을 묶어서 관리하기 위한 용도
        {
            public StaticObject2D LeftWall { get; private set; }
            public StaticObject2D RightWall { get; private set; }
            public StaticObject2D BottomWall { get; private set; }

            public IEnumerable<StaticObject2D> Walls
            {
                get
                {
                    yield return LeftWall;
                    yield return RightWall;
                    yield return BottomWall;
                }
            }

            public Basket(Vector2 center, Vector2 size, float wallThickness)
            {
                float halfWidth = size.X / 2f;
                float halfHeight = size.Y / 2f;

                LeftWall = new StaticObject2D(
                    new Vector2(center.X - halfWidth, center.Y),
                    new Vector2(wallThickness, size.Y),
                    0f,
                    EColliderType.RECTANGLE);

                RightWall = new StaticObject2D(
                    new Vector2(center.X + halfWidth, center.Y),
                    new Vector2(wallThickness, size.Y),
                    0f,
                    EColliderType.RECTANGLE);

                BottomWall = new StaticObject2D(
                    new Vector2(center.X, center.Y - halfHeight),
                    new Vector2(size.X + wallThickness, wallThickness),
                    0f,
                    EColliderType.RECTANGLE);
            }
        }

        #region rendering
        private void UpdateCanvasObjects()
        {
            ClearCanvas();

            // add objects in Canvas
            foreach (var obj in mWorld.Objects)
            {
                if (obj is Watermelon wObj)
                {
                    Shape shape;

                    if (wObj.Collider.Type == EColliderType.CIRCLE)
                    {
                        shape = new Ellipse()
                        {
                            Width = wObj.Collider.Size.X,
                            Height = wObj.Collider.Size.Y,
                        };

                        //Debug.Assert(mWatermelonLevelToVisual.ContainsKey(wObj.Level), "물체 레벨에 따른 이미지가 매핑되어 있어야 합니다.");

                        wObj.Collider.Size = mWatermelonLevelToVisual[wObj.Level]!.Size;

                        if (mWatermelonLevelToVisual[wObj.Level]!.ImgBrushOrNull == null)
                        {
                            Debug.WriteLine("현재 매핑되는 이미지가 없음.");
                            shape.Fill = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            shape.Fill = mWatermelonLevelToVisual[wObj.Level]!.ImgBrushOrNull;
                        }
                    }
                    else
                    {
                        Debug.Fail("현재 Watermelon은 Circle 타입의 2D 객체만 렌더링이 구현되어 있습니다.");
                        shape = null;
                    }

                    mainRenderngCanvas.Children.Add(shape);
                    renderObjects[wObj] = shape;
                }
                else if (obj is StaticObject2D)
                {
                    StaticObject2D sObj = (StaticObject2D)obj;
                    Shape shape;

                    if (sObj.Collider.Type == EColliderType.RECTANGLE)
                    {
                        shape = new System.Windows.Shapes.Rectangle()
                        {
                            Width = sObj.Collider.Size.X,
                            Height = sObj.Collider.Size.Y,
                        };

                        shape.Fill = new SolidColorBrush(Colors.Blue);
                        mainRenderngCanvas.Children.Add(shape);
                        renderObjects[sObj] = shape;
                    }
                    else if (sObj.Collider.Type == EColliderType.CIRCLE)
                    {
                        shape = new Ellipse()
                        {
                            Width = sObj.Collider.Size.X,
                            Height = sObj.Collider.Size.Y,
                        };

                        shape.Fill = new SolidColorBrush(Colors.Blue);
                        mainRenderngCanvas.Children.Add(shape);
                        renderObjects[sObj] = shape;
                    }
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
                shape.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
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
            if (mWorld.TryAddObject(new Watermelon(position, level)))
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

            for (int levelIndex = 0; levelIndex < Enum.GetValues(typeof(EWatermelonLevel)).Length; ++levelIndex)
            {
                Debug.Assert(mWatermelonLevelToVisual[(EWatermelonLevel)Enum.GetValues(typeof(EWatermelonLevel)).GetValue(levelIndex)!] != null);

                LevelVisual? currentVisual = mWatermelonLevelToVisual[(EWatermelonLevel)Enum.GetValues(typeof(EWatermelonLevel)).GetValue(levelIndex)!];
                string? targetImagePathOrNull = null;

                foreach (FileInfo img in imageFiles)
                {
                    if (System.IO.Path.GetFileNameWithoutExtension(img.Name) == (levelIndex + 1).ToString())
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

                currentVisual.ImgBrushOrNull = imgBrush;
            }
        }
        #endregion
    }
}
