using autoplaysharp.Contracts;

namespace autoplaysharp.Core.Game.Tasks.Missions.DualEpicQuests
{
    public class StupidXMen : GenericDualEpicQuest
    {
        public StupidXMen(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override string MissionName => "STUPID X-MEN";
    }
}
