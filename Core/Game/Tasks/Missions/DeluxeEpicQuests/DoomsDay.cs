using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;

namespace autoplaysharp.Core.Game.Tasks.Missions.DeluxeEpicQuests
{
    public class DoomsDay : GenericDeluxeEpicQuest
    {
        public DoomsDay(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override string MissionName => "DOOM'S DAY";
    }
}
