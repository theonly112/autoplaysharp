using System;
using System.Threading;
using System.Threading.Tasks;
using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using Microsoft.Extensions.Logging;

namespace autoplaysharp.Core.Game.Tasks
{
    internal class RestartGame : GameTask
    {
        public RestartGame(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            await WaitUntil(WaitForGameEntered, token, 120);
        }

        private async Task<bool> WaitForGameEntered(CancellationToken token)
        {
            if (Game.IsVisible(UIds.MAIN_MENU_ENTER) &&
                !Game.IsVisible(UIds.MAIN_MENU_STARTUP_STORE_NOTICE_X) &&
                !Game.IsVisible(UIds.MAIN_MENU_ALLIANCE_CONQUEST_HAS_BEGUN))
            {
                Game.Click(UIds.MAIN_MENU_ENTER);
                if (!await WaitUntilVisible(UIds.MAIN_MENU_SELECT_MISSION, token, 2))
                {
                    return false;
                }

                Game.Click(UIds.MAIN_MENU_HOME_BUTTON_IMAGE);

                if(!await WaitUntilVisible(UIds.MAIN_MENU_ENTER, token))
                {
                    return false;
                }

                Logger.LogInformation("Seem to have gotten back into main menu...");
                return true;
            }

            await CloseUpdateNotice(UIds.MAIN_MENU_STARTUP_UPDATE_NOTICE_X ,token);
            await CloseUpdateNotice(UIds.MAIN_MENU_STARTUP_UPDATE_NOTICE_X_2, token);
            await CloseUpdateNotice(UIds.MAIN_MENU_STARTUP_UPDATE_NOTICE_X_3, token);

            if (Game.IsVisible(UIds.MAIN_MENU_STARTUP_STORE_NOTICE_X))
            {
                await Task.Delay(500, token);
                Game.Click(UIds.MAIN_MENU_STARTUP_STORE_NOTICE_X);
                await Task.Delay(500, token);
                await ClickWhenVisible(UIds.MAIN_MENU_STARTUP_STORE_NOTICE_OK);
                Logger.LogInformation("Closing store notice.");
            }

            if (Game.IsVisible(UIds.MAIN_MENU_DOWNLOAD_UPDATE))
            {
                Game.Click(UIds.MAIN_MENU_DOWNLOAD_UPDATE);
                await Task.Delay(300, token);
                Logger.LogInformation("Game update available. Updating...");
            }

            if (Game.IsVisible(UIds.MAIN_MENU_ALLIANCE_CONQUEST_HAS_BEGUN))
            {
                Game.Click(UIds.MAIN_MENU_ALLIANCE_CONQUEST_HAS_BEGUN_CLOSE);
                await Task.Delay(300, token);
                Logger.LogInformation("Closing alliance conquest notification...");
            }

            return false;
        }

        private async Task CloseUpdateNotice(string id, CancellationToken token)
        {
            if (Game.IsVisible(id))
            {
                Game.Click(id);
                await Task.Delay(300, token);
                Logger.LogInformation("Closing update notice.");
            }
        }
    }
}
