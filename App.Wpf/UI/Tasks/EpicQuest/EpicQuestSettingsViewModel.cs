using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Contracts.Configuration.Tasks;
using autoplaysharp.Core.Game.Tasks.Missions;

namespace autoplaysharp.App.UI.Tasks.EpicQuest
{
    internal class EpicQuestSettingsViewModel<T> : TaskBaseViewModel<T> where  T : GenericEpicQuest
    {
        public EpicQuestSettingsViewModel(IGame game, IUiRepository repo, ITaskExecutioner taskExecutioner, ISettings settings) : base(game, repo, taskExecutioner, settings)
        {
            Settings = settings.EpicQuest;
        }

        public IEpicQuestSettings Settings { get; }

    }
}
