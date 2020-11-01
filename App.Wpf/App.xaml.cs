using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using App.Wpf;
using autoplaysharp.App.Logging;
using autoplaysharp.App.UI;
using autoplaysharp.App.UI.Repository;
using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Core.Game;
using autoplaysharp.Emulators;
using autoplaysharp.Game;
using autoplaysharp.Game.UI;
using autoplaysharp.Overlay;
using autoplaysharp.UiAutomation.OCR;
using Config.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace autoplaysharp.App.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static IServiceProvider ServiceProvider { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureSerivces(serviceCollection);
            serviceCollection.AddSingleton<MainWindow>();
            ServiceProvider = serviceCollection.BuildServiceProvider();
            var window = ServiceProvider.GetService<MainWindow>();
            window.Show();
        }

        private enum EmulatorType {
            NoxPlayer,
            BlueStacks
        }
        private class WindowSettings
        {
            public EmulatorType Emulator { get; set; }
            public string WindowName { get; set; }
        }

        private void ConfigureSerivces(ServiceCollection serviceCollection)
        {
            var uiLogger = new CustomUILoggerProvider();
            ILoggerFactory loggerFactory = LoggerFactory.Create(
                                (builder) =>
                                {
                                    builder
                                                .AddProvider(new CustomConsoleProvider())
                                                .AddProvider(new CustomTraceProvider())
                                                .AddProvider(uiLogger)
                                                .SetMinimumLevel(LogLevel.Debug);
                                });

            var config = Directory.CreateDirectory("Settings");
            var path = Path.Combine("Settings", "config.json");

            var setupDefaultConfig = !File.Exists(path);

            var settings = new ConfigurationBuilder<ISettings>()
                .UseJsonFile(path).
                Build();
            
            if(setupDefaultConfig)
            {
                SetupDefaultConfiguration(settings);
            }

            serviceCollection.AddSingleton(settings);

            var recognition = new TextRecognition();

            var executioner = new TaskExecutioner(loggerFactory.CreateLogger<TaskExecutioner>());
            IEmulatorWindow window = null;
            switch (settings.EmulatorType)
            {
                case Contracts.Configuration.EmulatorType.NoxPlayer:
                    window = new NoxWindow(settings.WindowName);
                    break;
                case Contracts.Configuration.EmulatorType.BlueStacks:
                    window = new BluestacksWindow(loggerFactory.CreateLogger<BluestacksWindow>(), recognition);
                    break;
                default:
                    throw new Exception("Invalid emulator type");
            }

            var repository = new Repository();
            repository.Load();
            var game = new GameImpl(window, repository, loggerFactory, recognition);
           
            var overlay = new ImGuiOverlay(game, window, repository);

            if (settings.EnableOverlay)
            {
                //circular dependency. find better solution.
                game.Overlay = overlay;
                overlay.Setup();
            }
            serviceCollection.AddSingleton<IEmulatorOverlay>(overlay);

            var picker = new AreaPicker(window, overlay);

            serviceCollection.AddSingleton<ITaskExecutioner>(executioner);
            serviceCollection.AddSingleton<ITaskQueue>(executioner);
            serviceCollection.AddSingleton<IGame>(game);
            serviceCollection.AddSingleton<IAreaPicker>(picker);
            serviceCollection.AddSingleton<IUiRepository>(repository);
            serviceCollection.AddSingleton<IEmulatorWindow>(window);
            serviceCollection.AddSingleton<IUiLogger>(uiLogger);
            ViewModelLocator.ConfigureServices(serviceCollection);


            if(settings.EnableOverlay)
            {
                Dispatcher.BeginInvoke(async () => await UpdateOverlay(overlay));
            }

            async Task UpdateOverlay(ImGuiOverlay overlay)
            {
                overlay.Update();
                await Task.Delay((int)(1000f / 60f));
#pragma warning disable CS4014 // We specifically to not want to await this call. We want this to run in the background.
                Dispatcher.BeginInvoke(async () => await UpdateOverlay(overlay));
#pragma warning restore CS4014 
            }
        }

        private static void SetupDefaultConfiguration(ISettings settings)
        {
            settings.WindowName = "NoxPlayer";
            settings.EmulatorType = Contracts.Configuration.EmulatorType.NoxPlayer;

            settings.TimelineBattle.Team = 1;

            settings.AllianceBattle.RunNormalMode = true;
            settings.AllianceBattle.RunExtremeMode = true;
        }
    }
}
