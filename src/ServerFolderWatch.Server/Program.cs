using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text.Json.Serialization;
using Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Server.Components;
using ServerFolderWatch.Server.DTOs;
using ServerFolderWatch.Server.PageModels;
using Testably.Abstractions;

namespace ServerFolderWatch.Server;

internal class Program
{
    private static ILogger<Program> logger;
    
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        AddConfiguration(builder);
        RegisterServices(builder);
        RegisterControllers(builder);
        RegisterRazorPages(builder);
        RegisterMappings();

        var app = BuildWebAppplication(builder);
        logger = app.Services.GetRequiredService<ILogger<Program>>();

        if (!InitializeApplication(app))
            return;

        logger.LogInformation("Taking recursive snapshot of public folder root.");
        app.Services.GetRequiredService<IFolderSnapshotService>()
            .TakeSnapshot(".", true);
        
        app.Run();
    }

    private static void AddConfiguration(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IAppConfiguration>(sp =>
            builder.Configuration
                .GetSection("App")
                .Get<AppConfiguration>() ??
            throw new InvalidOperationException("Configuration could not be loaded."));
    }

    private static void RegisterControllers(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers().AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    }

    private static void RegisterServices(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IFileSystem, RealFileSystem>();
        
        builder.Services.AddSingleton<IBrowseService, BrowseService>();
        builder.Services.AddSingleton<IFolderSnapshotService, SidecarFolderSnapshotService>();
        
        builder.Services.AddScoped<IFolderDiffService, FolderDiffService>();
    }
    
    private static void RegisterRazorPages(WebApplicationBuilder builder)
    {
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        
        builder.Services.AddScoped<BrowsePageModel>();
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
        var browseService = app.Services.GetRequiredService<IBrowseService>();
        string rootPublicPath = browseService.GetFileSystemPath(".");

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

    private static void RegisterMappings()
    {
        // TODO these models/DTOs are kind of duplicated?
        
        TypeAdapterConfig<FileSystemEntryBase, FileSystemEntryDto>
            .NewConfig()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Type, src => src is File
                ? FileSystemEntityType.File
                : FileSystemEntityType.Directory)
            .Map(dest => dest.Version, src => src is File
                ? (int?)((File)src).Version
                : null);

        TypeAdapterConfig<KeyValuePair<FileSystemEntryBase, DiffOperation>, FileSystemEntryDiffDto>
            .NewConfig()
            .Map(dest => dest.Name, src => src.Key.Name)
            .Map(dest => dest.Type, src => src is File
                ? FileSystemEntityType.File
                : FileSystemEntityType.Directory)
            .Map(dest => dest.DiffOperation, src => src.Value)
            .Map(dest => dest.Version, src => src.Key is File
                ? (int?)((File)src.Key).Version
                : null);
    }
}