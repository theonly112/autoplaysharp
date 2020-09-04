using autoplaysharp.Contracts;

namespace autoplaysharp.Core.Game.Tasks.Missions.DualEpicQuests
{
    public class TheBigTwin : GenericDualEpicQuest
    {
        public TheBigTwin(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override string MissionName => "THE BIG TWIN";
    }
}
