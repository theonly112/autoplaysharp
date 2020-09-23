using autoplaysharp.Contracts;

namespace autoplaysharp.App.UI.Tasks.DangerRoom
{
    internal class DangerRoomSettingsViewModel : TaskBaseViewModel<Game.Tasks.Missions.DangerRoom>
    {
        public DangerRoomSettingsViewModel(IGame game, IUiRepository repo, ITaskExecutioner taskExecutioner) : base(game, repo, taskExecutioner)
        {
        }
    }
}
