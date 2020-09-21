using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Contracts.Configuration.Tasks;

namespace autoplaysharp.App.UI.Tasks.AllianceBattle
{
    internal class AllianceBattleSettingsViewModel : TaskBaseViewModel<Game.Tasks.Missions.AllianceBattle>
    {
        private readonly ISettings _settings;

        public AllianceBattleSettingsViewModel(IGame game,
            IUiRepository repo,
            ITaskExecutioner executioner,
            ISettings settings) : base(game, repo, executioner)
        {
            _settings = settings;
        }

        public IAllianceBattleSettings Settings
        {
            get { return _settings.AllianceBattle; }
        }

        protected override IGameTask CreateTask()
        {
            var task = (Game.Tasks.Missions.AllianceBattle)base.CreateTask();
            task.ShouldRunNormalMode = Settings.RunNormalMode;
            task.ShouldRunExtremeMode = Settings.RunExtremeMode;
            return task;
        }
    }
}
