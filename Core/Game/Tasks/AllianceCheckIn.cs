using System.Threading;
using System.Threading.Tasks;
using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;

using Microsoft.Extensions.Logging;

namespace autoplaysharp.Core.Game.Tasks
{
    public class AllianceCheckIn : GameTask
    {
        public AllianceCheckIn(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            if (!await GoToMainScreen(token))
            {
                Logger.LogError("Could not go to main screen");
                return;
            }

            if (!await OpenMenu().ConfigureAwait(false))
            {
                Logger.LogError("Could not open main menu");
                return;
            }

            await ClickWhenVisible(UIds.MAIN_MENU_ALLIANCE_BUTTON);
            await ClickWhenVisible(UIds.ALLIANCE_CHECK_IN_BUTTON);
            await ClickWhenVisible(UIds.ALLIANCE_CHECK_IN_REWARD_OK);
            await GoToMainScreen(token);
        }
    }
}
