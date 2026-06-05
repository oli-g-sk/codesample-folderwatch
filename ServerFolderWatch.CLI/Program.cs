using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core.Service;

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
        
        var fileSystemWrapper = new FileSystem();
        var fileWrapper = new FileWrapper(fileSystemWrapper);
        var pathWrapper = new PathWrapper(fileSystemWrapper);
        var configuration = new Configuration();
        
        var persistenceService = new SidecarFilePersistenceService(fileWrapper, pathWrapper, configuration, loggerFactory);
        var browseService = new BrowseService(configuration, fileSystemWrapper);
        
        var fileSystemChangedService = new FileSystemDiffService(
            new FileSystem(),
            browseService,
            persistenceService,
            loggerFactory
        );
        
        Console.WriteLine("Enter path (defaults to C:\\Temp):");
        
        var input = Console.ReadLine();
        var path = TryReadPath(input);

        if (path is null)
        {
            Console.WriteLine("Invalid path. Using default.");
            path = configuration.RootPublicPath;
        }

        // TODO await
        var wasAlreadySetup = fileSystemChangedService.Analyze(path).Result;
        
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