using autoplaysharp.Contracts;

namespace autoplaysharp.App.UI.Tasks.AllianceBattle
{
    internal class AllianceBattleSettingsViewModel : TaskBaseViewModel<Game.Tasks.Missions.AllianceBattle>
    {
        public AllianceBattleSettingsViewModel(IGame game, IUiRepository repo, ITaskExecutioner executioner) : base(game, repo, executioner)
        {
        }

        public bool RunNormalMode { get; set; } = true;
        public bool RunExtremeMode { get; set; } = true;

        protected override IGameTask CreateTask()
        {
            var task = (Game.Tasks.Missions.AllianceBattle)base.CreateTask();
            task.ShouldRunNormalMode = RunNormalMode;
            task.ShouldRunExtremeMode = RunExtremeMode;
            return task;
        }
    }
}
