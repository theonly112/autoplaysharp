using autoplaysharp.Contracts;

namespace autoplaysharp.Core.Game.Tasks.Missions.DeluxeEpicQuests
{
    public class MutualEnemy : GenericDeluxeEpicQuest
    {
        public MutualEnemy(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override string MissionName => "MUTUAL ENEMY";
    }
}
