using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;

namespace autoplaysharp.Core.Game.Tasks.Missions.DeluxeEpicQuests
{
    public class BeginningOfTheChaos : GenericDeluxeEpicQuest
    {
        public BeginningOfTheChaos(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override string MissionName => "BEGINNING OF THE CHAOS";
    }
}
