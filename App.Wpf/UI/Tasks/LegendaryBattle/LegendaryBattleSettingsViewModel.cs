using autoplaysharp.Contracts;

namespace autoplaysharp.App.UI.Tasks.LegendaryBattle
{
    internal class LegendaryBattleSettingsViewModel : TaskBaseViewModel<Core.Game.Tasks.Missions.LegendaryBattle>
    {
        public LegendaryBattleSettingsViewModel(IGame game, IUiRepository repo, ITaskExecutioner taskExecutioner) : base(game, repo, taskExecutioner)
        {
        }
    }
}
