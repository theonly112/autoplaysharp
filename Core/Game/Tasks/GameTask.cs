﻿using autoplaysharp.Contracts;
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

        protected ILogger Logger;

        public GameTask(IGame game, IUiRepository repository)
        {
            Game = game;
            Repository = repository;
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
            return Game.IsVisible("MAIN_MENU_ENTER");
        }

        protected async Task<bool> OpenMenu()
        {
            if (!IsOnMainScreen())
            {
                Logger.LogError("Main screen not visible...");
                Logger.LogError("TODO: implement navigation to main screen");
                return false;
            }

            Game.Click("MAIN_MENU_MENU_BOTTON");
            await WaitUntilVisible("MAIN_MENU_MENU_HEADER").ConfigureAwait(false);
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
            if(Game.IsVisible("MAIN_MENU_HOME_BUTTON_IMAGE"))
            {
                Game.Click("MAIN_MENU_HOME_BUTTON_IMAGE");
            }

            if(!await WaitUntil(IsOnMainScreen))
            {
                return false;
            }
            await Task.Delay(500); // Screen is open but we should briefly wait before performing another action.
            return true;
        }


        protected Task<bool> HandleStartNotices()
        {
            if (Game.IsVisible("GENERIC_MISSION_INVENTORY_FULL_NOTICE"))
            {
                Logger.LogError("Inventory is full. TODO: at the moment this is not handled...");
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        protected async Task<bool> ClickWhenVisible(string uid)
        {
            if(!await WaitUntilVisible(uid))
            {
                return false;
            }
            
            Game.Click(uid);
            return true;
        }

     }
}
