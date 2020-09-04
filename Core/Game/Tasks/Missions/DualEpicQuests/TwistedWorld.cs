using autoplaysharp.Contracts;

namespace autoplaysharp.Core.Game.Tasks.Missions.DualEpicQuests
{
    public class TwistedWorld : GenericDualEpicQuest
    {
        public TwistedWorld(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override string MissionName => "TWISTED WORLD";
    }
}
