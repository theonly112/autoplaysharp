using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;

namespace autoplaysharp.App.UI.Tasks.WorldBossInvasion
{
    internal class WorldBossInvasionSettingsViewModel : TaskBaseViewModel<Core.Game.Tasks.Missions.WorldBossInvasion>
    {
        public WorldBossInvasionSettingsViewModel(IGame game, IUiRepository repo, ITaskExecutioner taskExecutioner, ISettings settings)
            : base(game, repo, taskExecutioner, settings)
        {
        }
    }
}
