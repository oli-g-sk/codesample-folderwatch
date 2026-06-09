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
        Console.ResetColor();
        
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
        
        PrintSingleLine("Folder" , fullPath);

        var oldSnapshot = snapshotService.LoadPersistedSnapshot(path);
        var currentContents = snapshotService.GetCurrentContents(path);
        var diff = diffService.Compare(oldSnapshot!, currentContents, path, out var summary);
        
        string lastAnalyzed = snapshotService.LoadPersistedSnapshot(fullPath)?.LastAnalyzed.ToString() ?? "NEVER";
        PrintSingleLine("Last snapshot", lastAnalyzed);
        
        if (wasMonitored)
            PrintSummaryLine(summary);
        
        PrintDiff(diff);
        
        Console.ResetColor();
        
        if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
        {
            if (args[0] == "commit")
            {
                Console.WriteLine($"Saving changes...");
                var snapshot = snapshotService.TakeSnapshot(fullPath, false).Result;
                Console.WriteLine($"Saved folder snapshot at: {snapshot}");
            }

            if (args[0] == "commitr")
            {
                Console.WriteLine($"Saving changes recursively...");
                snapshotService.TakeSnapshot(fullPath, true);
                Console.WriteLine("Saved recursive folder snapshot");
            }
        }
    }

    private static void PrintDiff(FolderSnapshotDiff diff)
    {
        foreach (var entry in diff.Entries)
        {
            if (entry.Operation == DiffOperation.Unchanged)
                continue;

            string character = "";
            Console.ForegroundColor = ConsoleColor.White;
            if (entry.Operation == DiffOperation.Added)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                character = "+";
            }

            if (entry.Operation == DiffOperation.Removed)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                character = "-";
            }

            if (entry.Operation == DiffOperation.Modified)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                character = "^";
            }

            string name = entry.FileSystemEntry is Folder
                ? $"{character} [{entry.FileSystemEntry.Name}]"
                : $"{character} {entry.FileSystemEntry.Name}";
            
            Console.WriteLine(name);
        }
    }

    private static void PrintSingleLine(string heading, string content)
    {
        Console.ResetColor();
        Console.Write($"{heading}: ");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write($"{content}");
        Console.WriteLine();
        Console.ResetColor();
    }

    private static void PrintSummaryLine(FolderSnapshotChanges changes)
    {
        if (!HasChanges(changes))
            PrintSingleLine("Summary","No changes");
        
        else
        {
            Console.Write("Summary: ");

            if (!changes.AddedEntries.Any() && !changes.DeletedEntries.Any() && !changes.ModifiedEntries.Any())
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("No changes");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                if (changes.AddedEntries.Any())
                    Console.Write($"ADD {changes.AddedEntries.Count} files ");
                Console.ForegroundColor = ConsoleColor.Red;
                if (changes.DeletedEntries.Any())
                    Console.Write($"REM {changes.DeletedEntries.Count} files ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                if (changes.ModifiedEntries.Any())
                    Console.Write($"MOD {changes.ModifiedEntries.Count} files ");

                Console.ResetColor();
                Console.WriteLine();
            }
        }
    }

    private static bool HasChanges(FolderSnapshotChanges diff)
    {
        return diff.AddedEntries.Count != 0
               || diff.DeletedEntries.Count != 0
               || diff.ModifiedEntries.Count != 0;
    }
}
