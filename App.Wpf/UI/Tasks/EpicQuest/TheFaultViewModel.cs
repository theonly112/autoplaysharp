using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Core.Game.Tasks.Missions.DualEpicQuests.Shifter;

namespace autoplaysharp.App.UI.Tasks.EpicQuest
{
    internal class TheFaultViewModel : EpicQuestSettingsViewModel<TheFault>
    {
        public TheFaultViewModel(IGame game, IUiRepository repo, ITaskExecutioner taskExecutioner, ISettings settings) : base(game, repo, taskExecutioner, settings)
        {
        }
    }
}
