using System;
using System.Threading;
using autoplaysharp.Game;
using autoplaysharp.Game.UI;
using autoplaysharp.Overlay;
using Microsoft.Extensions.Logging;

namespace autoplaysharp
{
    partial class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(
                (builder) =>
                  {
                      builder
                      .AddProvider(new CustomConsoleProvider())
                      .AddProvider(new CustomTraceProvider())
                      .SetMinimumLevel(LogLevel.Debug);
                  });

            var executioner = new TaskExecutioner(loggerFactory.CreateLogger<TaskExecutioner>());
            var noxWindow = new NoxWindow();
            var repository = new Repository();
            repository.Load();
            var game = new GameImpl(noxWindow, repository, loggerFactory);
            var overlay = new ImGuiOverlay(executioner, game, noxWindow, repository);
            // circular dependency. find better solution.
            game.Overlay = overlay;

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
