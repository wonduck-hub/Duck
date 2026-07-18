#include "pch.h"
#include "MainWindow.xaml.h"
#if __has_include("MainWindow.g.cpp")
#include "MainWindow.g.cpp"
#endif

using namespace winrt;
using namespace Microsoft::UI::Xaml;
using namespace Microsoft::UI::Xaml::Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace winrt::DHexEditor::implementation
{
    int32_t MainWindow::MyProperty()
    {
        throw hresult_not_implemented();
    }

    void MainWindow::MyProperty(int32_t /* value */)
    {
        throw hresult_not_implemented();
    }
}

void winrt::DHexEditor::implementation::MainWindow::ShowAbout_Click(winrt::Windows::Foundation::IInspectable const& sender, winrt::Microsoft::UI::Xaml::RoutedEventArgs const& e)
{
    ContentDialog aboutDialog = ContentDialog();

    aboutDialog.Title(box_value(L"About"));
    aboutDialog.Content(box_value(L"version: 0.0.1.0"));
    aboutDialog.PrimaryButtonText(L"OK");

    aboutDialog.XamlRoot(Content().XamlRoot());

    aboutDialog.ShowAsync();
}
