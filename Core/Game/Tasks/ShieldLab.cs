using autoplaysharp.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Game.Tasks
{
    public class ShieldLab : GameTask
    {
        public ShieldLab(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            await OpenMenu().ConfigureAwait(false);
            Game.Click("MAIN_MENU_SHIELD_LAB_BUTTON");
        }
    }
}
