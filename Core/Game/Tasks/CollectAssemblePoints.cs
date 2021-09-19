using System.Threading;
using System.Threading.Tasks;
using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;

using Microsoft.Extensions.Logging;

namespace autoplaysharp.Core.Game.Tasks
{
    public class CollectAssemblePoints : GameTask
    {
        public CollectAssemblePoints(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
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

            await ClickWhenVisible(UIds.MAIN_MENU_FRIENDS_BUTTON);
            await ClickWhenVisible(UIds.FRIENDS_SEND_ALL);
            await HandleEndNotices();
            for (int i = 0; i < 4; i++) // TODO: is 3 good here? what should we use?
            {
                await ClickWhenVisible(UIds.FRIENDS_ACQUIRE_ALL);
                await Task.Delay(1000, token);
                if (Game.IsVisible(UIds.FRIENDS_ACQUIRE_NOTICE_CANNOT_HAVE_MORE))
                {
                    Game.Click(UIds.FRIENDS_ACQUIRE_NOTICE_CANNOT_HAVE_MORE_OK);
                    await ClickWhenVisible(UIds.FRIENDS_ACQUIRE_PURCHASE_OK);
                }
                else
                {
                    if (!Game.IsVisible(UIds.FRIENDS_ACQUIRE_OK))
                    {
                        Logger.LogDebug("Already collected all points");
                        return;
                    }
                    await ClickWhenVisible(UIds.FRIENDS_ACQUIRE_OK);
                }
            }
            
            await GoToMainScreen(token);
        }
    }
}
