using System;
using System.Threading;
using autoplaysharp.Game;
using autoplaysharp.Game.UI;
using autoplaysharp.Overlay;
using Microsoft.Extensions.Logging;

namespace autoplaysharp
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(
                (builder) =>
                  {
                      builder.AddProvider(new CustomConsoleProvider());
                      builder.SetMinimumLevel(LogLevel.Debug);
                  });

            var executioner = new TaskExecutioner(loggerFactory.CreateLogger<TaskExecutioner>());
            var noxWindow = new NoxWindow();
            var repository = new Repository();
            repository.Load();
            var game = new GameImpl(noxWindow, repository, loggerFactory);
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

        class CustomConsoleProvider : ILoggerProvider
        {
            public ILogger CreateLogger(string categoryName)
            {
                return new CustomConsoleLogger(categoryName);
            }

            public void Dispose()
            {
                //throw new NotImplementedException();
            }
        }

        class CustomConsoleLogger : ILogger
        {
            private string _categoryName;

            public CustomConsoleLogger(string categoryName)
            {
                _categoryName = categoryName.Contains(".") ? categoryName.Substring(categoryName.LastIndexOf('.') + 1) : categoryName;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if(_categoryName != null)
                {
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {logLevel} - {_categoryName} - {formatter(state, exception)}");
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {logLevel} - {formatter(state, exception)}");
                }
            }
        }

    }
}
