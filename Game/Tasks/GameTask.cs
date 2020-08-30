using autoplaysharp.Contracts;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Game.Tasks
{
    internal abstract class GameTask
    {
        public event Action TaskEnded;
        protected IGame Game;
        public GameTask(IGame game)
        {
            Game = game;
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
                Console.WriteLine("Main screen not visible...");
                Console.WriteLine("TODO: implement navigation to main screen");
                return false;
            }

            Game.Click("MAIN_MENU_MENU_BOTTON");
            await WaitUntilVisible("MAIN_MENU_MENU_HEADER").ConfigureAwait(false);
            return true;
        }

        protected Task<bool> WaitUntilVisible(string id, float timeout = 5, float interval = 0.1f)
        {
            return WaitUntil(() => Game.IsVisible(id), timeout, interval);
        }

        protected async Task<bool> WaitUntil(Func<bool> condition, float timeout = 5, float interval = 0.1f)
        {
            var sw = Stopwatch.StartNew();
            while (!condition())
            {
                await Task.Delay((int)(interval * 1000)).ConfigureAwait(false);
                if (sw.ElapsedMilliseconds > (timeout * 1000))
                {
                    return false;
                }
            }
            return true;
        }

        protected Task<bool> GoToMainScreen()
        {
            if(Game.IsVisible("MAIN_MENU_HOME_BUTTON_IMAGE"))
            {
                Game.Click("MAIN_MENU_HOME_BUTTON_IMAGE");
            }

            return WaitUntil(IsOnMainScreen);
        }


        protected Task<bool> HandleStartNotices()
        {
            if(Game.IsVisible("GENERIC_MISSION_INVENTORY_FULL_NOTICE"))
            {
                Console.WriteLine("Inventory is full. TODO: at the moment this is not handled...");
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }
}
