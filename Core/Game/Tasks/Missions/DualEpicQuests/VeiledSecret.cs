using autoplaysharp.Contracts;

namespace autoplaysharp.Core.Game.Tasks.Missions.DualEpicQuests
{
    public class VeiledSecret : GenericDualEpicQuest
    {
        public VeiledSecret(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override string MissionName => "VEILED SECRET";
    }
}
