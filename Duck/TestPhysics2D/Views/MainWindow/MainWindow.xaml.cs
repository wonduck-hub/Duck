using DuckPhysics.Physics2D;
using Microsoft.Win32;
using System.Diagnostics;
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
        public MainWindow()
        {
            InitializeComponent();
            InitGame();
            InitUI();
        }

        private void InitUI()
        {
            GravityTextBox.Text = mWorld.Gravity.ToString();

            foreach (EWatermelonLevel level in Enum.GetValues(typeof(EWatermelonLevel)))
            {
                Vector2 size = mWatermelonLevelToVisual[level]!.Size;
                switch (level)
                {
                    case EWatermelonLevel.ZERO:
                        Level0TextBoxSize.Text = size.X.ToString();
                        break;
                    case EWatermelonLevel.ONE:
                        Level1TextBoxSize.Text = size.X.ToString();
                        break;
                    case EWatermelonLevel.TWO:
                        Level2TextBoxSize.Text = size.X.ToString();
                        break;
                    case EWatermelonLevel.THREE:
                        Level3TextBoxSize.Text = size.X.ToString();
                        break;
                    case EWatermelonLevel.FOUR:
                        Level4TextBoxSize.Text = size.X.ToString();
                        break;
                    case EWatermelonLevel.FIVE:
                        Level5TextBoxSize.Text = size.X.ToString();
                        break;
                    case EWatermelonLevel.SIX:
                        Level6TextBoxSize.Text = size.X.ToString();
                        break;
                    case EWatermelonLevel.SEVEN:
                        Level7TextBoxSize.Text = size.X.ToString();
                        break;
                    case EWatermelonLevel.EIGHT:
                        Level8TextBoxSize.Text = size.X.ToString();
                        break;
                    case EWatermelonLevel.NINE:
                        Level9TextBoxSize.Text = size.X.ToString();
                        break;
                    case EWatermelonLevel.TEN:
                        Level10TextBoxSize.Text = size.X.ToString();
                        break;
                    case EWatermelonLevel.ELEVEN:
                        Level11TextBoxSize.Text = size.X.ToString();
                        break;
                    default:
                        Debug.Fail("지원하지 않는 WatermelonLevel 입니다.");
                        break;
                }
            }
        }

        #region UI Event
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("초기화 하시겠습니까?", "초기화 확인", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                InitWorld2D();
                ClearCanvas();
                UpdateCanvasObjects();
                InitUI();
            }
        }

        private void MainCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(mainRenderngCanvas);

            float x = (float)pos.X;
            float y = (float)pos.Y;

            float dropHeight = 500f;

            AddNewFuruit(new System.Numerics.Vector2(x, dropHeight), EWatermelonLevel.ZERO);
        }

        private void ApplyGravityButton_Click(object sender, RoutedEventArgs e)
        {
            if (float.TryParse(GravityTextBox.Text, out float gravity))
            {
                mWorld.Gravity = gravity;
            }
            else
            {
                MessageBox.Show("유효한 숫자를 입력해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Error);
                GravityTextBox.Text = mWorld.Gravity.ToString();
            }
        }

        private void BrowseImageFolderButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderDialog = new OpenFolderDialog();

            if (folderDialog.ShowDialog() == true)
            {
                string selectedPath = folderDialog.FolderName;
                ImageFolderPathTextBox.Text = selectedPath;

                SetWatermellonImage(selectedPath);
            }
        }

        private void ApplyWatermellonSize_Click(object sender, RoutedEventArgs e)
        {
            float tempValue;
            if (float.TryParse(Level0TextBoxSize.Text, out tempValue))
            {
                mWatermelonLevelToVisual[EWatermelonLevel.ZERO]!.Size = new Vector2(tempValue, tempValue); // 현재 무조건 원형으로 가정
            }
            else
            {
                Debug.WriteLine($"Level 0 Size 입력값이 유효하지 않습니다: {Level0TextBoxSize.Text}");
            }

            if (float.TryParse(Level1TextBoxSize.Text, out tempValue))
            {
                mWatermelonLevelToVisual[EWatermelonLevel.ONE]!.Size = new Vector2(tempValue, tempValue);
            }
            else
            {
                Debug.WriteLine($"Level 1 Size 입력값이 유효하지 않습니다: {Level1TextBoxSize.Text}");
            }

            if (float.TryParse(Level2TextBoxSize.Text, out tempValue))
            {
                mWatermelonLevelToVisual[EWatermelonLevel.TWO]!.Size = new Vector2(tempValue, tempValue);
            }
            else
            {
                Debug.WriteLine($"Level 2 Size 입력값이 유효하지 않습니다: {Level2TextBoxSize.Text}");
            }

            if (float.TryParse(Level3TextBoxSize.Text, out tempValue))
            {
                mWatermelonLevelToVisual[EWatermelonLevel.THREE]!.Size = new Vector2(tempValue, tempValue);
            }
            else
            {
                Debug.WriteLine($"Level 3 Size 입력값이 유효하지 않습니다: {Level3TextBoxSize.Text}");
            }

            if (float.TryParse(Level4TextBoxSize.Text, out tempValue))
            {
                mWatermelonLevelToVisual[EWatermelonLevel.FOUR]!.Size = new Vector2(tempValue, tempValue);
            }
            else
            {
                Debug.WriteLine($"Level 4 Size 입력값이 유효하지 않습니다: {Level4TextBoxSize.Text}");
            }

            if (float.TryParse(Level5TextBoxSize.Text, out tempValue))
            {
                mWatermelonLevelToVisual[EWatermelonLevel.FIVE]!.Size = new Vector2(tempValue, tempValue);
            }
            else
            {
                Debug.WriteLine($"Level 5 Size 입력값이 유효하지 않습니다: {Level5TextBoxSize.Text}");
            }

            if (float.TryParse(Level6TextBoxSize.Text, out tempValue))
            {
                mWatermelonLevelToVisual[EWatermelonLevel.SIX]!.Size = new Vector2(tempValue, tempValue);
            }
            else
            {
                Debug.WriteLine($"Level 6 Size 입력값이 유효하지 않습니다: {Level6TextBoxSize.Text}");
            }

            if (float.TryParse(Level7TextBoxSize.Text, out tempValue))
            {
                mWatermelonLevelToVisual[EWatermelonLevel.SEVEN]!.Size = new Vector2(tempValue, tempValue);
            }
            else
            {
                Debug.WriteLine($"Level 7 Size 입력값이 유효하지 않습니다: {Level7TextBoxSize.Text}");
            }

            if (float.TryParse(Level8TextBoxSize.Text, out tempValue))
            {
                mWatermelonLevelToVisual[EWatermelonLevel.EIGHT]!.Size = new Vector2(tempValue, tempValue);
            }
            else
            {
                Debug.WriteLine($"Level 8 Size 입력값이 유효하지 않습니다: {Level8TextBoxSize.Text}");
            }

            if (float.TryParse(Level9TextBoxSize.Text, out tempValue))
            {
                mWatermelonLevelToVisual[EWatermelonLevel.NINE]!.Size = new Vector2(tempValue, tempValue);
            }
            else
            {
                Debug.WriteLine($"Level 9 Size 입력값이 유효하지 않습니다: {Level9TextBoxSize.Text}");
            }

            if (float.TryParse(Level10TextBoxSize.Text, out tempValue))
            {
                mWatermelonLevelToVisual[EWatermelonLevel.TEN]!.Size = new Vector2(tempValue, tempValue);
            }
            else
            {
                Debug.WriteLine($"Level 10 Size 입력값이 유효하지 않습니다: {Level10TextBoxSize.Text}");
            }

            if (float.TryParse(Level11TextBoxSize.Text, out tempValue))
            {
                mWatermelonLevelToVisual[EWatermelonLevel.ELEVEN]!.Size = new Vector2(tempValue, tempValue);
            }
            else
            {
                Debug.WriteLine($"Level 11 Size 입력값이 유효하지 않습니다: {Level11TextBoxSize.Text}");
            }
        }
        #endregion


    }
}
