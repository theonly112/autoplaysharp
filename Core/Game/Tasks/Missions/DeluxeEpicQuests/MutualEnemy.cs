using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;

namespace autoplaysharp.Core.Game.Tasks.Missions.DeluxeEpicQuests
{
    public class MutualEnemy : GenericDeluxeEpicQuest
    {
        public MutualEnemy(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override string MissionName => "MUTUAL ENEMY";
    }
}
