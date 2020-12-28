using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Core.Game.Tasks.Missions.DualEpicQuests.Shifter;

namespace autoplaysharp.App.UI.Tasks.EpicQuest
{
    internal class TwistedWorldViewModel : EpicQuestSettingsViewModel<TwistedWorld>
    {
        public TwistedWorldViewModel(IGame game, IUiRepository repo, ITaskExecutioner taskExecutioner, ISettings settings) : base(game, repo, taskExecutioner, settings)
        {
        }
    }
}
