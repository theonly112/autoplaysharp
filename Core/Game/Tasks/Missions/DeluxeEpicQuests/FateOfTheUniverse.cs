using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;

namespace autoplaysharp.Core.Game.Tasks.Missions.DeluxeEpicQuests
{
    public class FateOfTheUniverse : GenericDeluxeEpicQuest
    {
        public FateOfTheUniverse(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override string MissionName => "FATE OF THE UNIVERSE";
    }
}
