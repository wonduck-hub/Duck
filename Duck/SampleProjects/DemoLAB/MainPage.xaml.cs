

using Microsoft.Maui.Controls.Shapes;

namespace DemoLAB
{
    public partial class MainPage : ContentPage
    {
        // 임시로 사용할 Raw 데이터(CSV) 파일 경로

#if ANDROID
        private readonly string RAW_DATA_DIR_PATH = System.IO.Path.Combine(
        Android.OS.Environment.GetExternalStoragePublicDirectory(
            Android.OS.Environment.DirectoryDocuments
        ).AbsolutePath,
        "DemoLAB", "mRawData");
        private readonly string RAW_DATA_FILE_PATH = System.IO.Path.Combine(
        Android.OS.Environment.GetExternalStoragePublicDirectory(
            Android.OS.Environment.DirectoryDocuments
        ).AbsolutePath,
        "DemoLAB", "mRawData", "mRawData.csv");

#else
        private readonly string RAW_DATA_DIR_PATH = 
            System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                "DemoLAB", "mRawData");
        private readonly string RAW_DATA_FILE_PATH = 
            System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "DemoLAB", "mRawData", "mRawData.csv");
#endif
        private List<int> mRawData = new List<int>();

        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            RawPathLabel.Text = RAW_DATA_FILE_PATH;

            if (await CheckAndRequestStoragePermission())
            {
                if (!Directory.Exists(RAW_DATA_DIR_PATH))
                {
                    Directory.CreateDirectory(RAW_DATA_DIR_PATH);
                }

                if (!File.Exists(RAW_DATA_FILE_PATH))
                {
                    using (StreamWriter sr = new StreamWriter(
                        new FileStream(RAW_DATA_FILE_PATH, FileMode.Create, FileAccess.Write)))
                    {
                        sr.WriteLine("20,50,80,110,140,170,200,230,255");
                    }
                }
            }
            else
            {
                Application.Current.Quit();
            }
        }


        private async void ScreenShotBtn_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Success", "Save successed!", "OK");

        }

        private async void LoadBtn_Clicked(object sender, EventArgs e)
        {
            if (await CheckAndRequestStoragePermission())
            {
                try
                {
                    using (FileStream fs = new FileStream(RAW_DATA_FILE_PATH, FileMode.Open, FileAccess.Read))
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string? line = sr.ReadLine();
                        if (line == null)
                        {
                            await DisplayAlert("Error", "파일이 비어 있습니다.", "OK");
                            return;
                        }

                        string[] rawStr = line.Split(',');

                        foreach (string str in rawStr)
                        {
                            mRawData.Add(int.Parse(str.Trim())); // 파일 형식이 무조건 올바른거라 가정
                        }
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", ex.ToString(), "OK");
                }

                // 지금은 3 * 3 크기로 고정
                int rawIndex = 0;
                foreach (var rect in ImageGrid.Children)
                {
                    if (rect is Rectangle)
                    {
                        ((Rectangle)rect).Fill =
                            new SolidColorBrush(
                                Color.FromRgb(255, 255 - mRawData[rawIndex], 255 - mRawData[rawIndex]));
                    }

                    ++rawIndex;
                }
            }
            else
            {
                await DisplayAlert("권한 필요", "저장소 접근 권한이 필요합니다.", "OK");
            }
        }

        async Task<bool> CheckAndRequestStoragePermission()
        {
//#if ANDROID
        var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.StorageWrite>();
        }
        return status == PermissionStatus.Granted;
//#else
            //return true;
//#endif
        }

    }

}
