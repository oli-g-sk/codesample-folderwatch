using System.IO.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ServerFolderWatch.Core.Service;
using ServerFolderWatch.Server;
using IConfiguration = ServerFolderWatch.Core.IConfiguration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IConfiguration, Configuration>();
builder.Services.AddSingleton<IBrowseService, BrowseService>();
builder.Services.AddSingleton<IFileSystem, FileSystem>();
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();
