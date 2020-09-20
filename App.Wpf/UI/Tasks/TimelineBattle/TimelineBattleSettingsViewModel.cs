using autoplaysharp.Contracts;

namespace autoplaysharp.App.UI.Tasks.TimelineBattle
{
    internal class TimelineBattleSettingsViewModel : TaskBaseViewModel<Game.Tasks.Missions.TimelineBattle>
    {
        public TimelineBattleSettingsViewModel(IGame game, IUiRepository repo, ITaskExecutioner taskExecutioner) : base(game, repo, taskExecutioner)
        {
        }

        public int TeamIndex { get; set; }

        protected override IGameTask CreateTask()
        {
            var task = (Game.Tasks.Missions.TimelineBattle)base.CreateTask();
            task.Team = TeamIndex + 1;
            return task;
        }
    }
}
