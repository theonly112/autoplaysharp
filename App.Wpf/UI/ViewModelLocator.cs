using autoplaysharp.App.UI.Log;
using autoplaysharp.App.UI.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace autoplaysharp.App.UI
{
    internal class ViewModelLocator
    {
        public MainViewModel MainViewModel => Wpf.App.ServiceProvider.GetService<MainViewModel>();
        public RepositoryBrowserViewModel RepositoryBrowserViewModel => Wpf.App.ServiceProvider.GetService<RepositoryBrowserViewModel>();
        public LogViewModel LogViewModel => Wpf.App.ServiceProvider.GetService<LogViewModel>();

        internal static void ConfigureServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<MainViewModel>();
            serviceCollection.AddSingleton<RepositoryBrowserViewModel>();
            serviceCollection.AddSingleton<LogViewModel>();
        }
    }
}
