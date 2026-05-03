using CommunityToolkit.Mvvm.Input;
using WatermelonGameMobile.Models;

namespace WatermelonGameMobile.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}