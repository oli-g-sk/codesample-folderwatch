using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Core.Service;

public class FolderDiffService(
    IFileSystem fileSystem,
    IBrowseService browseService,
    ILoggerFactory loggerFactory) : IFolderDiffService
{
    private readonly ILogger<FolderDiffService> logger = loggerFactory.CreateLogger<FolderDiffService>();
    
    public Dictionary<FileSystemEntryBase, DiffOperation> Compare(FolderSnapshot? oldSnapshot, FolderSnapshot newSnapshot, string folderPath,
        out FolderSnapshotChanges changes)
    {
        changes = new FolderSnapshotChanges();
        var diff = new Dictionary<FileSystemEntryBase, DiffOperation>();

        if (oldSnapshot == null)
        {
            // No previous snapshot; diff will simply contain
            // all current items with the operation listed as "unchangeD"
            
            return newSnapshot.GetAllEntries()
                .ToDictionary(x => x, x => DiffOperation.Unchanged);
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

                if (FileHasChanged(currentEntry.Name, folderPath, oldSnapshot.LastAnalyzed))
                {
                    operation = DiffOperation.Modified;
                    changes.ModifiedEntries.Add(currentEntry);
                }
            }
            
            diff.Add(entry, operation);
        }
        
        if (!changes.AddedEntries.Any() && !changes.DeletedEntries.Any() && !changes.ModifiedEntries.Any())
            logger.LogDebug("No changes detected in {FolderPath}", folderPath);
        else
        
        if (changes.AddedEntries.Any())
            logger.LogDebug("Items added in {FolderPath}: {AddedFiles}", folderPath, changes.AddedEntries.Count);
        if (changes.DeletedEntries.Any())
            logger.LogDebug("Items deleted in {FolderPath}: {DeletedFiles}", folderPath, changes.DeletedEntries.Count);
        if (changes.ModifiedEntries.Any())
            logger.LogDebug("Items modified in {FolderPath}: {ModifiedFiles}", folderPath, changes.ModifiedEntries.Count);

        return diff;
    }
    
    public bool FileHasChanged(string fileName, string folderPath, DateTime? lastAnalyzed)
    {
        var fullPath = fileSystem.Path.Combine(browseService.GetFileSystemPath(folderPath), fileName);
        var modified = fileSystem.File.GetLastWriteTime(fullPath);

        // TODO other ways to detect changes? (size, MD5, etc.)
        return modified > lastAnalyzed;
    }
}
