using autoplaysharp.Contracts;

namespace autoplaysharp.Core.Game.Tasks.Missions.DeluxeEpicQuests
{
    public class FateOfTheUniverse : GenericDeluxeEpicQuest
    {
        public FateOfTheUniverse(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override string MissionName => "FATE OF THE UNIVERSE";
    }
}
