using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using autoplaysharp.App.Logging;
using autoplaysharp.App.UI;
using autoplaysharp.App.UI.Repository;
using autoplaysharp.App.UI.Setup;
using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Core.Game;
using autoplaysharp.Core.Game.UI;
using autoplaysharp.Emulators;
using autoplaysharp.Overlay;
using autoplaysharp.UiAutomation.OCR;
using Config.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace autoplaysharp.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static IServiceProvider ServiceProvider { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Otherwise application will close after setup window is closed...
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var serviceCollection = new ServiceCollection();
            ConfigureSerivces(serviceCollection);
            serviceCollection.AddSingleton<MainWindow>();
            ServiceProvider = serviceCollection.BuildServiceProvider();
            var window = ServiceProvider.GetService<MainWindow>();
            window.Show();
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
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
            window = SetupWindow(settings, loggerFactory, recognition);

            var repository = new Repository();
            repository.Load();
            var game = new GameImpl(window, repository, loggerFactory, recognition, settings);
           
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

        private static IEmulatorWindow SetupWindow(ISettings settings, ILoggerFactory loggerFactory,
            TextRecognition recognition)
        {
            IEmulatorWindow window;
            switch (settings.EmulatorType)
            {
                case Contracts.Configuration.EmulatorType.NoxPlayer:
                    window = new NoxWindow(settings.WindowName);
                    break;
                case Contracts.Configuration.EmulatorType.BlueStacks:
                    window = new BluestacksWindow(loggerFactory.CreateLogger<BluestacksWindow>(), recognition,
                        settings.WindowName);
                    break;
                default:
                    throw new Exception("Invalid emulator type");
            }

            try
            {
                window.Initialize();
            }
            catch (FailedToFindWindowException e)
            {
                var setup = new SetupWindow();
                var vm = new SetupViewModel(loggerFactory.CreateLogger<SetupViewModel>(), recognition, settings);
                setup.DataContext = vm;
                vm.Saved += () =>
                {
                    setup.DialogResult = true;
                    //setup.Hide();
                };

                if(setup.ShowDialog() != true)
                {
                    Current.Shutdown();
                }

                window = SetupWindow(settings, loggerFactory, recognition);
            }

            return window;
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
