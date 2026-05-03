using WatermelonGameMobile.Models;
using WatermelonGameMobile.PageModels;

namespace WatermelonGameMobile.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}