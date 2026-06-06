using System.IO.Abstractions;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ServerFolderWatch.Core.Service;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Server;
using IConfiguration = ServerFolderWatch.Core.IConfiguration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IFileSystem, FileSystem>();
builder.Services.AddSingleton<IConfiguration, Configuration>();
builder.Services.AddSingleton<IBrowseService, BrowseService>();
builder.Services.AddSingleton<IPersistenceService, SidecarFilePersistenceService>();

// TODO make scoped and provide path parameter in ctor?
//  otherwise it's unreliable and calling properties can throw NREs
builder.Services.AddSingleton<IFileSystemDiffService, FileSystemDiffService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());        
    });

var app = builder.Build();
app.MapControllers();
app.Run();
