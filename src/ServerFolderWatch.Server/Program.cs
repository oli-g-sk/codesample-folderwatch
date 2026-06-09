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
    private static ILogger<Program> logger;
    
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
        builder.Services.AddSingleton<IFolderSnapshotService, SidecarSnapshotService>();
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());        
            });

        var app = BuildWebAppplication(builder);
        logger = app.Services.GetRequiredService<ILogger<Program>>();

        if (InitializeApplication(app))
        {
            TakeStartupSnapshot(
                app.Services.GetRequiredService<IFolderSnapshotService>(),
                app.Services.GetRequiredService<IAppConfiguration>().RootPublicPath);

            app.Run();
        }
    }

    private static WebApplication BuildWebAppplication(WebApplicationBuilder builder)
    {
        var app = builder.Build();
        app.UseStaticFiles();
        app.UseAntiforgery();
        app.MapControllers();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return app;
    }

    private static bool InitializeApplication(WebApplication app)
    {
        var configuration = app.Services.GetRequiredService<IAppConfiguration>();
        string rootPublicPath = configuration.RootPublicPath;

        var browseService = app.Services.GetRequiredService<IBrowseService>();

        if (!browseService.FolderExists(rootPublicPath))
        {
            logger.LogError("Public folder path defined in configuration does not exist: {configurationPath}",
                rootPublicPath);
            return false;
        }

        if (!browseService.CanWriteToFolder(rootPublicPath))
        {
            logger.LogError("Public folder path defined in configuration is not writeable: {configurationPath}",
                rootPublicPath);
            return false;
        }

        return true;
    }
    
    private static void TakeStartupSnapshot(IFolderSnapshotService snapshotService, string rootPublicPath)
    {
        logger.LogInformation("Taking recursive snapshot of public folder: {rootPublicPath}", rootPublicPath);
        snapshotService.TakeSnapshot(rootPublicPath, true).Wait();
    }
}
