using System;
using System.Threading;
using autoplaysharp.Game;
using autoplaysharp.Game.UI;
using autoplaysharp.Overlay;

namespace autoplaysharp
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var executioner = new TaskExecutioner();
            var noxWindow = new NoxWindow();
            var repository = new Repository();
            repository.Load();
            var game = new GameImpl(noxWindow, repository);
            var overlay = new ImGuiOverlay(executioner, game, noxWindow, repository);

            while (true)
            {
                game.Update();
                executioner.Update();
                overlay.Update();
                // how can we reduce cpu impact...
                Thread.Sleep(5);
            }
        }
    }
}
