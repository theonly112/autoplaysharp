using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;

namespace autoplaysharp.App.UI.Tasks.CoopMission
{
    internal class CoopMissionSettingsViewModel : TaskBaseViewModel<Game.Tasks.Missions.CoopMission>
    {
        public CoopMissionSettingsViewModel(IGame game, IUiRepository repo, ITaskExecutioner taskExecutioner, ISettings settings)
            : base(game, repo, taskExecutioner, settings)
        {
        }
    }
}
