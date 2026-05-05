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

        private void SelectedLevelComboBoxItemChanged(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ComboBoxLevel.SelectedItem.ToString()))
            {
                return;
            }

            TextBoxNewSize.Text = mWatermelonLevelToVisual[(EWatermelonLevel)ComboBoxLevel.SelectedIndex]!.Size.X.ToString();
        }

        private void SetWatermellonSizeOfLevel(object sender, RoutedEventArgs e)
        {
            if (ComboBoxLevel.SelectedIndex > mWatermelonLevelToVisual.Count)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(TextBoxNewSize.Text))
            {
                MessageBox.Show("값이 유효하지 않습니다.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            float temp;

            if (!float.TryParse(TextBoxNewSize.Text, out temp))
            {
                MessageBox.Show("값이 유효하지 않습니다.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            mWatermelonLevelToVisual[(EWatermelonLevel)ComboBoxLevel.SelectedIndex]!.Size = new Vector2(temp, temp);
        }
        #endregion


    }
}
