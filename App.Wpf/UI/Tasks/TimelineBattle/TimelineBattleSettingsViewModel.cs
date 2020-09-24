using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Contracts.Configuration.Tasks;

namespace autoplaysharp.App.UI.Tasks.TimelineBattle
{
    internal class TimelineBattleSettingsViewModel : TaskBaseViewModel<Game.Tasks.Missions.TimelineBattle>
    {
        private readonly ISettings _settings;

        public TimelineBattleSettingsViewModel(
            IGame game,
            IUiRepository repo,
            ITaskExecutioner taskExecutioner,
            ISettings settings) : base(game, repo, taskExecutioner, settings)
        {
            _settings = settings;
        }

        public ITimelineBattleSettings Settings
        {
            get { return _settings.TimelineBattle; }
        }

        protected override IGameTask CreateTask()
        {
            var task = (Game.Tasks.Missions.TimelineBattle)base.CreateTask();
            task.Team = Settings.Team;
            return task;
        }
    }
}
