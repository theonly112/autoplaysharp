﻿using System;
using System.Threading;
using System.Threading.Tasks;
using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Game.Tasks;

namespace autoplaysharp.Core.Game.Tasks
{
    internal class RestartGame : GameTask
    {
        public RestartGame(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            var t1 = ClickWhenVisible(UIds.MAIN_MENU_STARTUP_UPDATE_NOTICE_X, 60);
            var t2 = ClickWhenVisible(UIds.MAIN_MENU_STARTUP_STORE_NOTICE_X, 60)
                .ContinueWith(async task =>
                {
                    if (await task)
                    {
                        await Task.Delay(300);
                        Game.Click(UIds.MAIN_MENU_STARTUP_STORE_NOTICE_OK);
                    }
                }, token);
            if (!await WaitUntilVisible(UIds.MAIN_MENU_ENTER, token, 60))
            {
                await t1;
                await t2;
            }
        }
    }
}