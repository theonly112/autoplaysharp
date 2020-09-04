using autoplaysharp.Contracts;

namespace autoplaysharp.Game.Tasks.Missions
{
    public class StupidXMen : GenericDualEpicQuest
    {
        public StupidXMen(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override string MissionName => "STUPID X-MEN";
    }
}
