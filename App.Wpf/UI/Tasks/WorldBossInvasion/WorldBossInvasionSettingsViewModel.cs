using autoplaysharp.Contracts;

namespace autoplaysharp.App.UI.Tasks.WorldBossInvasion
{
    internal class WorldBossInvasionSettingsViewModel : TaskBaseViewModel<Core.Game.Tasks.Missions.WorldBossInvasion>
    {
        public WorldBossInvasionSettingsViewModel(IGame game, IUiRepository repo, ITaskExecutioner taskExecutioner) : base(game, repo, taskExecutioner)
        {
        }
    }
}
