using autoplaysharp.Contracts;

namespace autoplaysharp.Core.Game.Tasks.Missions.DeluxeEpicQuests
{
    public class BeginningOfTheChaos : GenericDeluxeEpicQuest
    {
        public BeginningOfTheChaos(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override string MissionName => "BEGINNING OF THE CHAOS";
    }
}
