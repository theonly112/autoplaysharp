using autoplaysharp.Contracts;

namespace autoplaysharp.Core.Game.Tasks.Missions.DeluxeEpicQuests
{
    public class DoomsDay : GenericDeluxeEpicQuest
    {
        public DoomsDay(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override string MissionName => "DOOM'S DAY";
    }
}
