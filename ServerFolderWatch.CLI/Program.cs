using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core;

namespace ServerFolderWatch.CLI;

class Program
{
    private static readonly string DefaultPath = @"C:\Temp";
    
    static void Main(string[] args)
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        
        var fileSystem = new FileSystem();
        
        var fileSystemChangedService = new FileSystemChangedService(
            new PathWrapper(fileSystem),
            new DirectoryWrapper(fileSystem),
            new FileWrapper(fileSystem),
            new Configuration(),
            loggerFactory
        );
        
        Console.WriteLine("Enter path (defaults to C:\\Temp):");
        
        var input = Console.ReadLine();
        var path = TryReadPath(input);

        if (path is null)
        {
            Console.WriteLine("Invalid path. Using default.");
            path = DefaultPath;
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