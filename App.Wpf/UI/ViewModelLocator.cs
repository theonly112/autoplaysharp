using autoplaysharp.App.UI.DebugView;
using autoplaysharp.App.UI.Log;
using autoplaysharp.App.UI.Repository;
using autoplaysharp.App.UI.Setup;
using autoplaysharp.App.UI.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace autoplaysharp.App.UI
{
    internal class ViewModelLocator
    {
        public MainViewModel MainViewModel => App.ServiceProvider.GetService<MainViewModel>();
        public RepositoryBrowserViewModel RepositoryBrowserViewModel => App.ServiceProvider.GetService<RepositoryBrowserViewModel>();
        public LogViewModel LogViewModel => App.ServiceProvider.GetService<LogViewModel>();
        public TasksViewModel TasksViewModel => App.ServiceProvider.GetService<TasksViewModel>();
        public DebugViewModel DebugViewModel => App.ServiceProvider.GetService<DebugViewModel>();

        internal static void ConfigureServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<MainViewModel>();
            serviceCollection.AddSingleton<RepositoryBrowserViewModel>();
            serviceCollection.AddSingleton<LogViewModel>();
            serviceCollection.AddSingleton<TasksViewModel>();
            serviceCollection.AddSingleton<DebugViewModel>();
            serviceCollection.AddSingleton<SetupViewModel>();

            SettingsViewModels.ConfigureServices(serviceCollection);
        }
    }
}
