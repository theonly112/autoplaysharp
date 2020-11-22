using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Core.Game.Tasks.Missions.DualEpicQuests;
using autoplaysharp.Core.Game.Tasks.Missions.DualEpicQuests.Shifter;

namespace autoplaysharp.App.UI.Tasks.EpicQuest
{
    internal class StupidXMenViewModel : EpicQuestSettingsViewModel<StupidXMen>
    {
        public StupidXMenViewModel(IGame game, IUiRepository repo, ITaskExecutioner taskExecutioner, ISettings settings) : base(game, repo, taskExecutioner, settings)
        {
        }
    }
}
