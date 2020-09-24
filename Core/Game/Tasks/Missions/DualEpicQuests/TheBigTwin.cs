using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using System.Collections.Generic;

namespace autoplaysharp.Core.Game.Tasks.Missions.DualEpicQuests
{
    public class TheBigTwin : GenericDualEpicQuest
    {
        public TheBigTwin(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override string MissionName => "THE BIG TWIN";
    }
}
