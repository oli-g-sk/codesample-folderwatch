using System.IO.Abstractions;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ServerFolderWatch.Core.Service;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Server;
using ServerFolderWatch.Server.Components;
using Testably.Abstractions;
using IConfiguration = ServerFolderWatch.Core.IConfiguration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IFileSystem, RealFileSystem>();
builder.Services.AddSingleton<IConfiguration, Configuration>();
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

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

var configuration = app.Services.GetRequiredService<IConfiguration>();
var snapshotService = app.Services.GetRequiredService<IFolderSnapshotService>();
string rootPath = configuration.RootPublicPath;
snapshotService.InitializeFolder(rootPath, true);

app.Run();
