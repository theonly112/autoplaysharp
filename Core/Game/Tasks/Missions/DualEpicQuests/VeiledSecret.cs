﻿using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;

namespace autoplaysharp.Core.Game.Tasks.Missions.DualEpicQuests
{
    public class VeiledSecret : GenericDualEpicQuest
    {
        public VeiledSecret(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override string MissionName => "VEILED SECRET";
    }
}
