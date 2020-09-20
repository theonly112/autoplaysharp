using autoplaysharp.Contracts;
using autoplaysharp.Game.Tasks.Missions;

namespace autoplaysharp.App.UI.Tasks
{
    internal class AllianceBattleViewModel : TaskBaseViewModel<AllianceBattle>
    {
        public AllianceBattleViewModel(IGame game, IUiRepository repo, ITaskExecutioner executioner) : base(game, repo, executioner)
        {
        }

        public bool RunNormalMode { get; set; } = true;
        public bool RunExtremeMode { get; set; } = true;

        protected override IGameTask CreateTask()
        {
            var task = (AllianceBattle)base.CreateTask();
            task.ShouldRunNormalMode = RunNormalMode;
            task.ShouldRunExtremeMode = RunExtremeMode;
            return task;
        }
    }
}
