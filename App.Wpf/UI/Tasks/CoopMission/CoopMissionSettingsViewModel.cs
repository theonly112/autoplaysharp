using autoplaysharp.Contracts;

namespace autoplaysharp.App.UI.Tasks.CoopMission
{
    internal class CoopMissionSettingsViewModel : TaskBaseViewModel<Game.Tasks.Missions.CoopMission>
    {
        public CoopMissionSettingsViewModel(IGame game, IUiRepository repo, ITaskExecutioner taskExecutioner) : base(game, repo, taskExecutioner)
        {
        }
    }
}
