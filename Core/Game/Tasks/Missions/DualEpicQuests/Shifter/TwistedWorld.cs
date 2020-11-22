using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;

namespace autoplaysharp.Core.Game.Tasks.Missions.DualEpicQuests.Shifter
{
    public class TwistedWorld : ShifterEpicQuest
    {
        public TwistedWorld(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override string MissionName => "TWISTED WORLD";
    }
}
