using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service;
using Testably.Abstractions;

namespace ServerFolderWatch.CLI;

class Program
{
    static void Main(string[] args)
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            // builder.AddConsole();
#if DEBUG
            builder.SetMinimumLevel(LogLevel.Trace);
#endif
        });
        
        var fileSystemWrapper = new RealFileSystem();
        var configuration = new AppConfiguration();
        var browseService = new BrowseService(configuration, fileSystemWrapper);
        var snapshotService = new SidecarSnapshotService(browseService,
            configuration, fileSystemWrapper, loggerFactory);
        var diffService = new FolderDiffService(fileSystemWrapper, browseService, loggerFactory);
        
        Console.WriteLine("Enter path (leave empty for default:");
        
        var input = Console.ReadLine();
        var path = TryReadPath(input, browseService);

        if (path is null)
        {
            Console.WriteLine("Path not provided or invalid; Using default.");
            path = ".";
        }

        if (!snapshotService.IsFolderAlreadyMonitored(path))
        {
            Console.WriteLine("Setup complete.");
            return;
        }

        var diff = diffService.Compare(snapshotService.LoadPersistedSnapshot(path),
            snapshotService.GetCurrentContents(path), path, out var diffs);

        foreach (var entry in diff.Entries)
        {
            Console.ForegroundColor = ConsoleColor.White;
            if (entry.Operation == DiffOperation.Added)
                Console.ForegroundColor = ConsoleColor.Green;
            if (entry.Operation == DiffOperation.Removed)
                Console.ForegroundColor = ConsoleColor.Red;
            if (entry.Operation == DiffOperation.Modified)
                Console.ForegroundColor = ConsoleColor.Yellow;

            string name = entry.FileSystemEntry is Folder
                ? $"[{entry.FileSystemEntry.Name}]"
                : entry.FileSystemEntry.Name;
            
            Console.WriteLine(name);
        }
        
        snapshotService.TakeSnapshot(path, true).Wait();
        
        Console.WriteLine();
        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }

    static string? TryReadPath(string? input, BrowseService browseService)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        if (!browseService.FolderExists(input))
            return null;

        return input;
    }
}
