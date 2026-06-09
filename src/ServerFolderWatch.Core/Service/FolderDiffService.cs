using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Core.Service;

public class FolderDiffService(IFileSystem fileSystem, ILoggerFactory loggerFactory) : IFolderDiffService
{
    private readonly ILogger<FolderDiffService> logger = loggerFactory.CreateLogger<FolderDiffService>();
    
    public FolderSnapshotDiff Compare(FolderSnapshot? oldSnapshot, FolderSnapshot newSnapshot, string folderPath,
        out FolderSnapshotChanges changes)
    {
        changes = new FolderSnapshotChanges();
        var diff = new FolderSnapshotDiff();

        if (oldSnapshot == null)
        {
            // No previous snapshot; diff will simply contain
            // all current items with the operation listed as "unchangeD"
            
            diff.Entries.AddRange(newSnapshot.GetAllEntries()
                .Select(x => (x, DiffOperation.Unchanged)));
            
            return diff;
        }

        var currentEntries = newSnapshot.GetAllEntries();
        var previousEntries = oldSnapshot.GetAllEntries();
        var combined = currentEntries.Union(previousEntries).Order();
        
        foreach (var entry in combined)
        {
            DiffOperation operation = DiffOperation.Unchanged;

            if (currentEntries.Contains(entry) && !previousEntries.Contains(entry))
            {
                changes.AddedEntries.Add(entry);
                operation = DiffOperation.Added;
            }
            else if (previousEntries.Contains(entry) && !currentEntries.Contains(entry))
            {
                changes.DeletedEntries.Add(entry);
                operation = DiffOperation.Removed;
            }
            else if (entry is File)
            {
                var previousEntry = previousEntries.First(x => x.Equals(entry)) as File;
                var currentEntry = currentEntries.First(x => x.Equals(entry)) as File;
                currentEntry!.Version = previousEntry!.Version;

                if (FileHasChanged(currentEntry, oldSnapshot, folderPath))
                {
                    operation = DiffOperation.Modified;
                    changes.ModifiedEntries.Add(currentEntry);
                    currentEntry!.Version++;
                }
            }
            
            diff.Entries.Add((entry, operation));
        }
        
        if (!changes.AddedEntries.Any() && !changes.DeletedEntries.Any() && !changes.ModifiedEntries.Any())
            logger.LogInformation("No changes detected in {FolderPath}", folderPath);
        else
        
        if (changes.AddedEntries.Any())
            logger.LogInformation("Items added in {FolderPath}: {AddedFiles}", folderPath, changes.AddedEntries.Count);
        if (changes.DeletedEntries.Any())
            logger.LogInformation("Items deleted in {FolderPath}: {DeletedFiles}", folderPath, changes.DeletedEntries.Count);
        if (changes.ModifiedEntries.Any())
            logger.LogInformation("Items modified in {FolderPath}: {ModifiedFiles}", folderPath, changes.ModifiedEntries.Count);

        newSnapshot.LastAnalyzed = DateTime.Now;
        
        return diff;
    }
    
    private bool FileHasChanged(File fileEntry, FolderSnapshot oldSnapshot, string folderPath)
    {
        var fullPath = fileSystem.Path.Combine(folderPath, fileEntry.Name);
        var modified = fileSystem.File.GetLastWriteTime(fullPath);

        // TODO other ways to detect changes? (size, MD5, etc.)
        return modified > oldSnapshot.LastAnalyzed;
    }
}