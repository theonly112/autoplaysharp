using System.Threading;
using System.Threading.Tasks;
using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;

namespace autoplaysharp.Core.Game.Tasks
{
    public class ShieldLab : GameTask
    {
        public ShieldLab(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            if(!await GoToMainScreen())
            {

            }
            await OpenMenu().ConfigureAwait(false);
            Game.Click(UIds.MAIN_MENU_SHIELD_LAB_BUTTON);

            await Task.Delay(1000);

            Game.Click(UIds.SHIELD_LAB_COLLECT_ANTIMATTER);

            // TODO: stuff...
        }
    }
}
