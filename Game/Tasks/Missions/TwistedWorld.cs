using autoplaysharp.Contracts;

namespace autoplaysharp.Game.Tasks.Missions
{
    class TwistedWorld : GenericDualEpicQuest
    {
        public TwistedWorld(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override string MissionName => "TWISTED WORLD";
    }
}
