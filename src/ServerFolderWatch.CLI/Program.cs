using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service;
using ServerFolderWatch.Core.Service.Interfaces;
using Testably.Abstractions;

namespace ServerFolderWatch.CLI;

class Program
{
    private static IFolderSnapshotService snapshotService;
    private static IFolderDiffService diffService;
    
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
        
        snapshotService = new SidecarSnapshotService(browseService,
            configuration, fileSystemWrapper, loggerFactory);
        diffService = new FolderDiffService(fileSystemWrapper, browseService, loggerFactory);
       
        string path = configuration.RootPublicPath;
        string fullPath = browseService.GetFileSystemPath(path);
        bool wasMonitored = snapshotService.IsFolderAlreadyMonitored(fullPath);
        
        Console.WriteLine("Folder: " + fullPath);
        string lastAnalyzed = snapshotService.LoadPersistedSnapshot(fullPath)?.LastAnalyzed.ToString() ?? "NEVER";
        Console.WriteLine("Last snapshot: " + lastAnalyzed);
        Console.WriteLine();
        
        if (wasMonitored)
            PrintDiff(fullPath);
        
        if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
        {
            if (args[0] == "commit")
            {
                snapshotService.TakeSnapshot(fullPath, false);
                Console.WriteLine("Saved folder snapshot");
            }

            if (args[0] == "commitr")
            {
                snapshotService.TakeSnapshot(fullPath, true);
                Console.WriteLine("Saved recursive folder snapshot");
            }
        }
    }

    private static void PrintDiff(string path)
    {
        var oldSnapshot = snapshotService.LoadPersistedSnapshot(path);
        var currentContents = snapshotService.GetCurrentContents(path);
        var diff = diffService.Compare(oldSnapshot, currentContents, path, out _);
        
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
    }
}
