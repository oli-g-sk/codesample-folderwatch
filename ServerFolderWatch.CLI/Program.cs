using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core.Service;
using Testably.Abstractions;

namespace ServerFolderWatch.CLI;

class Program
{
    static void Main(string[] args)
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
#if DEBUG
            builder.SetMinimumLevel(LogLevel.Trace);
#endif
        });
        
        var fileSystemWrapper = new RealFileSystem();
        var configuration = new Configuration();
        
        var snapshotService = new JsonFolderSnapshotService(fileSystemWrapper, configuration, loggerFactory);
        var browseService = new BrowseService(configuration, fileSystemWrapper);
        
        Console.WriteLine("Enter path (defaults to C:\\Temp):");
        
        var input = Console.ReadLine();
        var path = TryReadPath(input);

        if (path is null)
        {
            Console.WriteLine("Invalid path. Using default.");
            path = configuration.RootPublicPath;
        }

        // TODO await
        var wasAlreadySetup = snapshotService.IsFolderAlreadyMonitored(path);
        
        if (!wasAlreadySetup)
        {
            Console.WriteLine("Setup complete.");
            return;
        }
    }

    static string? TryReadPath(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        if (!Path.Exists(input))
            return null;

        return input;
    }
}