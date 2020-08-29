using autoplaysharp.Contracts;

namespace autoplaysharp.Game.Tasks.Missions
{
    class TheFault : GenericDualEpicQuest
    {
        public TheFault(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override string MissionName => "THE FAULT";
    }
}
