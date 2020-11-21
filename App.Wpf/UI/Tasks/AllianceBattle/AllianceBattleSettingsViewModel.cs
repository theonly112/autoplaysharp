using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Contracts.Configuration.Tasks;

namespace autoplaysharp.App.UI.Tasks.AllianceBattle
{
    internal class AllianceBattleSettingsViewModel : TaskBaseViewModel<Core.Game.Tasks.Missions.AllianceBattle>
    {
        private readonly ISettings _settings;

        public AllianceBattleSettingsViewModel(IGame game,
            IUiRepository repo,
            ITaskExecutioner executioner,
            ISettings settings) : base(game, repo, executioner, settings)
        {
            _settings = settings;
        }

        public IAllianceBattleSettings Settings
        {
            get { return _settings.AllianceBattle; }
        }
    }
}
