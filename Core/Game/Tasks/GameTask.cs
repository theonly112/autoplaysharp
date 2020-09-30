using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Contracts.Errors;
using autoplaysharp.Game.UI;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Game.Tasks
{
    public abstract class GameTask : IGameTask
    {
        public event Action TaskEnded;
        protected readonly IGame Game;
        protected readonly IUiRepository Repository;
        protected readonly ISettings Settings;
        protected ILogger Logger;

        public GameTask(IGame game, IUiRepository repository, ISettings settings)
        {
            Game = game;
            Repository = repository;
            Settings = settings;
            Logger = Game.CreateLogger(GetType());
        }

        public async Task Run(CancellationToken token)
        {
            try
            {
                await RunCore(token).ConfigureAwait(false);
            }
            finally
            {
                TaskEnded?.Invoke();
            }
        }
        protected abstract Task RunCore(CancellationToken token);


        /* 
         *  Some Common Tasks...
         */


        protected bool IsOnMainScreen()
        {
            // TODO: make this more robust... enter is visible in other places...
            return Game.IsVisible(UIds.MAIN_MENU_ENTER);
        }

        protected async Task<bool> OpenMenu()
        {
            if (!IsOnMainScreen())
            {
                Logger.LogError("Main screen not visible...");
                Logger.LogError("TODO: implement navigation to main screen");
                return false;
            }

            Game.Click(UIds.MAIN_MENU_MENU_BOTTON);
            await WaitUntilVisible(UIds.MAIN_MENU_MENU_HEADER).ConfigureAwait(false);
            return true;
        }

        protected Task<bool> WaitUntilVisible(string id, CancellationToken token, float timeout = 5, float interval = 0.1f)
        {
            return WaitUntilVisible(Repository[id], token, timeout, interval);
        }

        protected Task<bool> WaitUntilVisible(string id, float timeout = 5, float interval = 0.1f)
        {
            return WaitUntilVisible(Repository[id], timeout, interval);
        }

        protected Task<bool> WaitUntilVisible(UIElement element, float timeout = 5, float interval = 0.1f)
        {
            return WaitUntilVisible(element, CancellationToken.None, timeout, interval);
        }

        protected Task<bool> WaitUntilVisible(UIElement element, CancellationToken token, float timeout = 5, float interval = 0.1f)
        {
            return WaitUntil(() => Game.IsVisible(element), token, timeout, interval);
        }

        protected async Task<bool> WaitUntil(Func<bool> condition, CancellationToken token, float timeout = 5, float interval = 0.1f)
        {
            var sw = Stopwatch.StartNew();
            while (!condition())
            {
                await Task.Delay((int)(interval * 1000)).ConfigureAwait(false);
                if (sw.ElapsedMilliseconds > (timeout * 1000))
                {
                    return false;
                }

                if(token.IsCancellationRequested)
                {
                    return false;
                }
            }
            return true;
        }

        protected Task<bool> WaitUntil(Func<bool> condition, float timeout = 5, float interval = 0.1f)
        {
            return WaitUntil(condition, CancellationToken.None, timeout, interval);
        }

        protected Task<bool> GoToMainScreen()
        {
            return GoToMainScreen(CancellationToken.None);
        }

        protected async Task<bool> GoToMainScreen(CancellationToken token)
        {
            // TOOD: is this the right place?
            await HandleEndNotices();

            if (Game.IsVisible(UIds.MAIN_MENU_HOME_BUTTON_IMAGE))
            {
                Game.Click(UIds.MAIN_MENU_HOME_BUTTON_IMAGE);
                await Task.Delay(1000);
            }

            await HandleEndNotices();

            if(!await WaitUntil(IsOnMainScreen, token))
            {
                Game.OnError(new ElementNotFoundError(Repository[UIds.MAIN_MENU_ENTER]));
                return false;
            }
            await Task.Delay(500, token); // Screen is open but we should briefly wait before performing another action.
            return true;
        }


        protected Task<bool> HandleStartNotices()
        {
            if (Game.IsVisible(UIds.GENERIC_MISSION_INVENTORY_FULL_NOTICE))
            {
                Logger.LogError("Inventory is full. TODO: at the moment this is not handled...");
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        protected async Task HandleEndNotices()
        {
            await Task.Delay(2000); // Give it some time for end messages to appear.
            if(Game.IsVisible(UIds.CHALLENGES_DAILY_NOTIFICATION_CHALLLENGES))
            {
                // Collect daily challenge rewards?
                Game.Click(UIds.CHALLENGES_DAILY_NOTIFICATION_CHALLLENGES_OK);
                await Task.Delay(1000);
            }

            if (Game.IsVisible(UIds.HEROIC_QUEST_FINISHED_NOTICE))
            {
                await HandleHeroicQuestNotice();
            }
        }

        protected async Task<bool> HandleHeroicQuestNotice(int timeout = 5)
        {
            Logger.LogDebug("Waiting for heroic quest notice to appear.");
            if (await WaitUntilVisible(UIds.HEROIC_QUEST_FINISHED_NOTICE, timeout))
            {
                Logger.LogDebug("Heroic quest notice appeared. Closing it.");
                await Task.Delay(500);
                Game.Click(UIds.HEROIC_QUEST_FINISHED_NOTICE);
                await ClickWhenVisible(UIds.HEROIC_QUEST_QUEST_INFO_ACQUIRE);
                await Task.Delay(500);
                await ClickWhenVisible(UIds.HEROIC_QUEST_TAB_THE_SCREEN_TO_CONTINUE);
                await Task.Delay(1000);
                return true;
            }
            else
            {
                Logger.LogDebug("Heroic quest notice did not appeared.");
                return false;
            }
        }

        protected async Task<bool> ClickWhenVisible(string uid, int timeout = 5)
        {
            if(!await WaitUntilVisible(uid, timeout))
            {
                Game.OnError(new ElementNotFoundError(Repository[uid]));
                return false;
            }

            // Even if it is visible. its a good idea to wait briefly before clicking. 
            // Otherwise the click might not register.
            await Task.Delay(250);
            Game.Click(uid);
            return true;
        }
     }
}
