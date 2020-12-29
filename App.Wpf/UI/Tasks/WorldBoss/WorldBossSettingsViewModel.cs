using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;

namespace autoplaysharp.App.UI.Tasks.WorldBoss
{
    internal class WorldBossSettingsViewModel : TaskBaseViewModel<Core.Game.Tasks.Missions.WorldBoss>
    {
        public WorldBossSettingsViewModel(IGame game, IUiRepository repo, ITaskExecutioner taskExecutioner, ISettings settings) : base()
        {
        }
    }
}
