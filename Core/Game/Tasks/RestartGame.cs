using System;
using System.Threading;
using System.Threading.Tasks;
using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;

namespace autoplaysharp.Core.Game.Tasks
{
    internal class RestartGame : GameTask
    {
        public RestartGame(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            var t1 = WaitUntilVisible(UIds.MAIN_MENU_STARTUP_UPDATE_NOTICE_X, token, 60).ContinueWith(
                async task =>
                {
                    if (await task)
                    {
                        Game.Click(UIds.MAIN_MENU_STARTUP_UPDATE_NOTICE_X);
                        await Task.Delay(300, token);
                    }
                }, token);
            var t2 = WaitUntilVisible(UIds.MAIN_MENU_STARTUP_STORE_NOTICE_X, token, 60)
                .ContinueWith(async task =>
                {
                    if (await task)
                    {
                        await Task.Delay(300, token);
                        Game.Click(UIds.MAIN_MENU_STARTUP_STORE_NOTICE_X);
                        await Task.Delay(300, token);
                        Game.Click(UIds.MAIN_MENU_STARTUP_STORE_NOTICE_OK);
                    }
                }, token);
            if (!await WaitUntilVisible(UIds.MAIN_MENU_ENTER, token, 60))
            {
                if (Game.IsVisible(UIds.MAIN_MENU_STARTUP_STORE_NOTICE_X))
                {
                    await Task.Delay(300, token);
                    Game.Click(UIds.MAIN_MENU_STARTUP_STORE_NOTICE_X);
                    await Task.Delay(300, token);
                    Game.Click(UIds.MAIN_MENU_STARTUP_STORE_NOTICE_OK);
                }
                await t1;
                await t2;
            }
        }
    }
}
