using autoplaysharp.Contracts;

namespace autoplaysharp.App.UI.Tasks.SquadBattle
{
    internal class SquadBattleSettingsViewModel : TaskBaseViewModel<Core.Game.Tasks.Missions.LegendaryBattle>
    {
        public SquadBattleSettingsViewModel(IGame game, IUiRepository repo, ITaskExecutioner taskExecutioner) : base(game, repo, taskExecutioner)
        {
        }
    }
}
