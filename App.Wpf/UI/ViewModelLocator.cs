using autoplaysharp.App.UI.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace autoplaysharp.App.UI
{
    internal class ViewModelLocator
    {
        public MainViewModel MainViewModel => Wpf.App.ServiceProvider.GetService<MainViewModel>();
        public RepositoryBrowserViewModel RepositoryBrowserViewModel => Wpf.App.ServiceProvider.GetService<RepositoryBrowserViewModel>();

        internal static void ConfigureServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<MainViewModel>();
            serviceCollection.AddSingleton<RepositoryBrowserViewModel>();
        }
    }
}
