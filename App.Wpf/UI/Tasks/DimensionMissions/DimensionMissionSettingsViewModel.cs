using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Contracts.Configuration.Tasks;

namespace autoplaysharp.App.UI.Tasks.DimensionMissions
{
    internal class DimensionMissionSettingsViewModel : TaskBaseViewModel<Core.Game.Tasks.Missions.DimensionMission>
    {
        public DimensionMissionSettingsViewModel(
            IGame game,
            IUiRepository repo,
            ITaskExecutioner taskExecutioner,
            ISettings settings) : base(game, repo, taskExecutioner, settings)
        {
            Settings = settings.DimensionMission;
        }

        public IDimensionMissionSettings Settings { get; }
    }
}
