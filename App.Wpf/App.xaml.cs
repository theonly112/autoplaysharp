using App.Wpf;
using autoplaysharp.App.UI;
using autoplaysharp.App.UI.Repository;
using autoplaysharp.Contracts;
using autoplaysharp.Game;
using autoplaysharp.Game.UI;
using autoplaysharp.Overlay;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

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

        private void ConfigureSerivces(ServiceCollection serviceCollection)
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
            //circular dependency. find better solution.
            game.Overlay = overlay;
            serviceCollection.AddSingleton<IEmulatorOverlay>(overlay);

            var picker = new AreaPicker(noxWindow, overlay);

            serviceCollection.AddSingleton<IGame>(game);
            serviceCollection.AddSingleton<IAreaPicker>(picker);
            serviceCollection.AddSingleton<IUiRepository>(repository);
            serviceCollection.AddSingleton<IEmulatorWindow>(noxWindow);
            ViewModelLocator.ConfigureServices(serviceCollection);



            Dispatcher.BeginInvoke(async () => await UpdateOverlay(overlay));

            async Task UpdateOverlay(ImGuiOverlay overlay)
            {
                overlay.Update();
                await Task.Delay((int)(1000f / 60f));
#pragma warning disable CS4014 // We specifically to not want to await this call. We want this to run in the background.
                Dispatcher.BeginInvoke(async () => await UpdateOverlay(overlay));
#pragma warning restore CS4014 
            }
        }
    }
}
