using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;

namespace autoplaysharp.App.UI.Tasks.HeroicQuest
{
    internal class HeroicQuestSettingsViewModel : TaskBaseViewModel<Core.Game.Tasks.Missions.HeroicQuest>
    {
        public HeroicQuestSettingsViewModel(IGame game, IUiRepository repo, ITaskExecutioner taskExecutioner, ISettings settings) 
            : base()
        {
        }
    }
}
