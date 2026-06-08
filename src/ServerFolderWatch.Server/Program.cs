using System;
using System.IO.Abstractions;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core;
using ServerFolderWatch.Core.Service;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Server;
using ServerFolderWatch.Server.Components;
using Testably.Abstractions;

internal class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<IAppConfiguration>(sp =>
            builder.Configuration
                .GetSection("App")
                .Get<AppConfiguration>() ??
            throw new InvalidOperationException("Configuration could not be loaded."));

        builder.Services.AddSingleton<IFileSystem, RealFileSystem>();
        
        // TODO use scoped lifecycles?
        builder.Services.AddSingleton<IBrowseService, BrowseService>();
        builder.Services.AddSingleton<IFolderDiffService, FolderDiffService>();
        builder.Services.AddSingleton<IFolderSnapshotService, JsonFolderSnapshotService>();
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());        
            });

        var app = BuildWebApp(builder);
        ConfigureWebApp(app);

        TakeStartupSnapshot(
            app.Services.GetRequiredService<IFileSystem>(),
            app.Services.GetRequiredService<IAppConfiguration>().RootPublicPath);

        app.Run();
    }

    private static WebApplication BuildWebApp(WebApplicationBuilder builder)
    {
        var app = builder.Build();
        app.UseStaticFiles();
        app.UseAntiforgery();
        app.MapControllers();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return app;
    }

    private static void ConfigureWebApp(WebApplication app)
    {
        var configuration = app.Services.GetRequiredService<IAppConfiguration>();
        string rootPublicPath = configuration.RootPublicPath;

        var browseService = app.Services.GetRequiredService<IBrowseService>();
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        var fileSystem = app.Services.GetRequiredService<IFileSystem>();

        if (!fileSystem.Directory.Exists(rootPublicPath))
            logger.LogError("Public folder path defined in configuration does not exist: {configurationPath}", rootPublicPath);
        if (!browseService.CanWriteToFolder(rootPublicPath))
            logger.LogError("Public folder path defined in configuration is not writeable: {configurationPath}", rootPublicPath);
    }
    
    private static void TakeStartupSnapshot(IFileSystem fileSystem, string rootPublicPath)
    {
        var snapshotService = new JsonFolderSnapshotService(
            new BrowseService(new AppConfiguration(), fileSystem),
            fileSystem,
            new AppConfiguration(),
            new LoggerFactory());
     
        // TODO if not initialized, commit
        snapshotService.InitializeFolder(rootPublicPath, true);
    }
}
