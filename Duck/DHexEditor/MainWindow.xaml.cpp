#include "pch.h"
#include "MainWindow.xaml.h"
#if __has_include("MainWindow.g.cpp")
#include "MainWindow.g.cpp"
#endif

using namespace winrt;
using namespace Microsoft::UI::Xaml;
using namespace Microsoft::UI::Xaml::Controls;
using namespace Microsoft::Windows::Storage::Pickers;

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

void winrt::DHexEditor::implementation::MainWindow::ShowAbout_Click(
    winrt::Windows::Foundation::IInspectable const&, winrt::Microsoft::UI::Xaml::RoutedEventArgs const&)
{
    ContentDialog aboutDialog = ContentDialog();

    aboutDialog.Title(box_value(L"About"));
    aboutDialog.Content(box_value(L"version: 0.0.1.0"));
    aboutDialog.PrimaryButtonText(L"OK");

    aboutDialog.XamlRoot(Content().XamlRoot());

    aboutDialog.ShowAsync();
}

winrt::fire_and_forget winrt::DHexEditor::implementation::MainWindow::OpenFile_Click(
    winrt::Windows::Foundation::IInspectable const&, winrt::Microsoft::UI::Xaml::RoutedEventArgs const&)
{
    FileOpenPicker picker = FileOpenPicker(AppWindow().Id());

    picker.Title(L"Open File");
    picker.SuggestedStartLocation(
        PickerLocationId::DocumentsLibrary);

    // 필터를 지정하지 않으면 모든 파일 표시
    auto result{ co_await picker.PickSingleFileAsync() };

    if (!result)
    {
        co_return;
    }

    std::filesystem::path path = result.Path().c_str();

    mHexDocument = new DHexDocument(path);
}
