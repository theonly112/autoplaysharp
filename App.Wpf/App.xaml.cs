using System;
using System.IO;
using System.Linq;
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
    public partial class App
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
            window?.Show();

            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private void ConfigureSerivces(ServiceCollection serviceCollection)
        {
            var uiLogger = new CustomUiLoggerProvider();
            ILoggerFactory loggerFactory = LoggerFactory.Create(
                                (builder) =>
                                {
                                    builder
                                                .AddProvider(new CustomConsoleProvider())
                                                .AddProvider(new CustomTraceProvider())
                                                .AddProvider(uiLogger)
                                                .SetMinimumLevel(LogLevel.Debug);
                                });

            Directory.CreateDirectory("Settings");
            var path = Path.Combine("Settings", "config.json");

            var setupDefaultConfig = !File.Exists(path);

            var settings = new ConfigurationBuilder<ISettings>()
                .UseJsonFile(path).
                Build();
            
            if(setupDefaultConfig)
            {
                SetupDefaultConfiguration(settings);
            }
            else
            {
                EnsureValidValues(settings);
            }

            serviceCollection.AddSingleton(settings);

            var recognition = new TextRecognition();

            var executioner = new TaskExecutioner(loggerFactory.CreateLogger<TaskExecutioner>());
            var window = SetupWindow(settings, loggerFactory, recognition);
            var videoProvider = new VideoProvider(window, settings.VideoCapture.FrameRate);
            serviceCollection.AddSingleton<IVideoProvider>(videoProvider);
            serviceCollection.AddSingleton<IVideoCapture, VideoCapture>();

            var repository = new Repository();
            repository.Load();
            var game = new GameImpl(window, videoProvider, repository, loggerFactory, recognition, settings);
           
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
            serviceCollection.AddSingleton(window);
            serviceCollection.AddSingleton<IUiLogger>(uiLogger);
            ViewModelLocator.ConfigureServices(serviceCollection);

            if(settings.EnableOverlay)
            {
                Dispatcher.BeginInvoke(async () => await UpdateOverlay(overlay));
            }
        }

        private static void EnsureValidValues(ISettings settings)
        {
            settings.VideoCapture.RecordingDir ??= "recording";
            settings.VideoCapture.FrameRate = Math.Max(10, settings.VideoCapture.FrameRate);
        }

        private async Task UpdateOverlay(ImGuiOverlayBase overlay)
        {
            overlay.Update();
            await Task.Delay((int)(1000f / 60f));
#pragma warning disable CS4014 // We specifically to not want to await this call. We want this to run in the background.
            Dispatcher.BeginInvoke(async () => await UpdateOverlay(overlay));
#pragma warning restore CS4014
        }

        private static IEmulatorWindow SetupWindow(ISettings settings, ILoggerFactory loggerFactory,
            ITextRecognition recognition)
        {
            IEmulatorWindow window = settings.EmulatorType switch
            {
                    EmulatorType.NoxPlayer => new NoxWindow(settings.WindowName),
                    EmulatorType.BlueStacks => new BluestacksWindow(loggerFactory.CreateLogger<BluestacksWindow>(), recognition, settings.WindowName),
                    _ => throw new Exception("Invalid emulator type")
            };

            try
            {
                window.Initialize();
            }
            catch (FailedToFindWindowException)
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
            settings.EmulatorType = EmulatorType.NoxPlayer;
            settings.VideoCapture.RecordingDir = "recording";
            settings.VideoCapture.FrameRate = 30;
            settings.TimelineBattle.Team = 1;

            settings.AllianceBattle.RunNormalMode = true;
            settings.AllianceBattle.RunExtremeMode = true;

            settings.EpicQuest.RestartForBioFarming = false;
            settings.EpicQuest.UseClearTickets = true;

            settings.RoutineItems = new[]
                {
                    typeof(Core.Game.Tasks.AllianceCheckIn),
                    typeof(Core.Game.Tasks.CollectAssemblePoints),
                    typeof(Core.Game.Tasks.Missions.DispatchMission),
                    typeof(Core.Game.Tasks.Missions.AllianceBattle),
                    typeof(Core.Game.Tasks.Missions.AllianceBattle),
                    typeof(Core.Game.Tasks.Missions.CoopMission),
                    typeof(Core.Game.Tasks.Missions.DailyTrivia),
                    typeof(Core.Game.Tasks.Missions.DangerRoom),
                    typeof(Core.Game.Tasks.Missions.DimensionMission),
                    typeof(Core.Game.Tasks.Missions.LegendaryBattle),
                    typeof(Core.Game.Tasks.Missions.SquadBattle),
                    typeof(Core.Game.Tasks.Missions.TimelineBattle),
                    typeof(Core.Game.Tasks.Missions.WorldBossInvasion),
                    typeof(Core.Game.Tasks.Missions.WorldBoss),
                    typeof(Core.Game.Tasks.Missions.DeluxeEpicQuests.BeginningOfTheChaos),
                    typeof(Core.Game.Tasks.Missions.DeluxeEpicQuests.DoomsDay),
                    typeof(Core.Game.Tasks.Missions.DeluxeEpicQuests.FateOfTheUniverse),
                    typeof(Core.Game.Tasks.Missions.DeluxeEpicQuests.MutualEnemy),
                    typeof(Core.Game.Tasks.Missions.DeluxeEpicQuests.PlayingHero),
                    typeof(Core.Game.Tasks.Missions.DualEpicQuests.Shifter.StupidXMen),
                    typeof(Core.Game.Tasks.Missions.DualEpicQuests.Shifter.TheFault),
                    typeof(Core.Game.Tasks.Missions.DualEpicQuests.Shifter.TwistedWorld),
                    typeof(Core.Game.Tasks.Missions.DualEpicQuests.TheBigTwin),
                    typeof(Core.Game.Tasks.Missions.DualEpicQuests.VeiledSecret),
                }
                .Select(x => x.Namespace).ToArray();
            settings.RoutineItemsState = Enumerable.Repeat("true", settings.RoutineItems.Length).ToArray();
        }
    }
}
