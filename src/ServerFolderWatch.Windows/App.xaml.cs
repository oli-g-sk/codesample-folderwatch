using System.IO.Abstractions;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ServerFolderWatch.Core;
using ServerFolderWatch.Core.Service;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Desktop.ViewModels;
using Testably.Abstractions;

namespace ServerFolderWatch.Windows;

public partial class App : Application
{
    private ServiceProvider? serviceProvider;

    public IServiceProvider Services =>
        serviceProvider ?? throw new InvalidOperationException(
            "The service provider is not available before application startup.");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);

        serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        MainWindow = Services.GetRequiredService<MainWindow>();
        MainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        serviceProvider?.Dispose();
        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAppConfiguration, AppConfiguration>();
        services.AddSingleton<IFileSystem, RealFileSystem>();
        services.AddSingleton<IBrowseService, BrowseService>();

        services.AddSingleton<FolderTreeViewModel>();
        services.AddSingleton<BrowseViewModel>();
        services.AddSingleton<MainWindow>();
    }
}
