using System;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Contracts.Configuration.Tasks;

namespace autoplaysharp.App.UI.Tasks.EpicQuest
{
    internal class EpicQuestSettingsViewModel : TaskBaseViewModel
    {
        public EpicQuestSettingsViewModel(ISettings settings)
        {
            Settings = settings.EpicQuest;
        }

        public IEpicQuestSettings Settings { get; }

        public override string Name { get; }
        public override Type TaskType { get; }
    }
}
