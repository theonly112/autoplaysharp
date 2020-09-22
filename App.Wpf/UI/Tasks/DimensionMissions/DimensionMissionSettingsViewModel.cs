using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Contracts.Configuration.Tasks;
using autoplaysharp.Core.Game.Tasks.Missions;

namespace autoplaysharp.App.UI.Tasks.DimensionMissions
{
    internal class DimensionMissionSettingsViewModel : TaskBaseViewModel<Core.Game.Tasks.Missions.DimensionMission>
    {
        public DimensionMissionSettingsViewModel(
            IGame game,
            IUiRepository repo,
            ITaskExecutioner taskExecutioner,
            ISettings settings) : base(game, repo, taskExecutioner)
        {
            Settings = settings.DimensionMission;
        }

        public IDimensionMissionSettings Settings { get; }

        protected override IGameTask CreateTask()
        {
            var task = (DimensionMission)base.CreateTask();
            task.CollectRewardCount = Settings.RewardsToCollect;
            return task;
        }
    }
}
