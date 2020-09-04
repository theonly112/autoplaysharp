using autoplaysharp.Contracts;

namespace autoplaysharp.Core.Game.Tasks.Missions.DualEpicQuests
{
    public class TheFault : GenericDualEpicQuest
    {
        public TheFault(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override string MissionName => "THE FAULT";
    }
}
