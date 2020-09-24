using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;

namespace autoplaysharp.App.UI.Tasks.SquadBattle
{
    internal class SquadBattleSettingsViewModel : TaskBaseViewModel<Core.Game.Tasks.Missions.SquadBattle>
    {
        public SquadBattleSettingsViewModel(IGame game, IUiRepository repo, ITaskExecutioner taskExecutioner, ISettings settings)
            : base(game, repo, taskExecutioner, settings)
        {
        }
    }
}
