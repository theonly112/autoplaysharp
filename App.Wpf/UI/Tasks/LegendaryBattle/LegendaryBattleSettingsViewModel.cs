using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Contracts.Configuration.Tasks;

namespace autoplaysharp.App.UI.Tasks.LegendaryBattle
{
    internal class LegendaryBattleSettingsViewModel : TaskBaseViewModel<Core.Game.Tasks.Missions.LegendaryBattle>
    {
        public LegendaryBattleSettingsViewModel(
            IGame game,
            IUiRepository repo,
            ITaskExecutioner taskExecutioner,
            ISettings settings) : base()
        {
            Settings = settings.LegendaryBattle;
        }

        public ILegendaryBattleSettings Settings { get; }
    }
}
