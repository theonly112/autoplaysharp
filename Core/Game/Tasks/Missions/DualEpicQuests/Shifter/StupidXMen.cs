using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;

namespace autoplaysharp.Core.Game.Tasks.Missions.DualEpicQuests.Shifter
{
    public class StupidXMen : ShifterEpicQuest
    {
        public StupidXMen(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override string MissionName => "STUPID X-MEN";
    }
}
