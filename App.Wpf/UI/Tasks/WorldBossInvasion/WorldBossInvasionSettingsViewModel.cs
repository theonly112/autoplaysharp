using autoplaysharp.Contracts;

namespace autoplaysharp.App.UI.Tasks.WorldBossInvasion
{
    internal class WorldBossInvasionSettingsViewModel : TaskBaseViewModel<Core.Game.Tasks.Missions.LegendaryBattle>
    {
        public WorldBossInvasionSettingsViewModel(IGame game, IUiRepository repo, ITaskExecutioner taskExecutioner) : base(game, repo, taskExecutioner)
        {
        }
    }
}
