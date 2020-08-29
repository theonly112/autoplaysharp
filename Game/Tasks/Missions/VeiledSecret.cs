using autoplaysharp.Contracts;

namespace autoplaysharp.Game.Tasks.Missions
{
    internal class VeiledSecret : GenericDualEpicQuest
    {
        public VeiledSecret(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override string MissionName => "VEILED SECRET";
    }
}
