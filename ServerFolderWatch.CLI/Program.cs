using System.IO.Abstractions;
using ServerFolderWatch.Core;

namespace ServerFolderWatch.CLI;

class Program
{
    private static readonly string DefaultPath = @"C:\Temp";
    
    static void Main(string[] args)
    {
        var fileSystem = new FileSystem();
        
        var fileSystemChangedService = new FileSystemChangedService(
            new PathWrapper(fileSystem),
            new DirectoryWrapper(fileSystem),
            new FileWrapper(fileSystem)
        );
        
        Console.WriteLine("Enter path (defaults to C:\\Temp):");
        
        var input = Console.ReadLine();
        var path = TryReadPath(input);

        if (path is null)
        {
            Console.WriteLine("Invalid path. Using default.");
            path = DefaultPath;
        }

        if (!fileSystemChangedService.IsSetup(path))
        {
            Console.WriteLine("Setting up...");
            // TODO await
            fileSystemChangedService.Setup(path).Wait();
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