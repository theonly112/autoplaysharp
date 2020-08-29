using autoplaysharp.Contracts;

namespace autoplaysharp.Game.Tasks.Missions
{
    class TheBigTwin : GenericDualEpicQuest
    {
        public TheBigTwin(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override string MissionName => "THE BIG TWIN";
    }
}
