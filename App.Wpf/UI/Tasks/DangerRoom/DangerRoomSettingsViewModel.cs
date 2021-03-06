﻿using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;

namespace autoplaysharp.App.UI.Tasks.DangerRoom
{
    internal class DangerRoomSettingsViewModel : TaskBaseViewModel<Core.Game.Tasks.Missions.DangerRoom>
    {
        public DangerRoomSettingsViewModel(IGame game, IUiRepository repo, ITaskExecutioner taskExecutioner, ISettings settings)
            : base()
        {
        }
    }
}
