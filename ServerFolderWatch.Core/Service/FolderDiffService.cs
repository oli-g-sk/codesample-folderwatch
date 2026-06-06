using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Core.Service;

public class FolderDiffService(IFileSystem fileSystem, ILoggerFactory loggerFactory) : IFolderDiffService
{
    private readonly ILogger<FolderDiffService> logger = loggerFactory.CreateLogger<FolderDiffService>();

    public FolderSnapshotChanges Diff(FolderSnapshot? oldSnapshot, FolderSnapshot newSnapshot, string folderPath)
    {
        var diff = new FolderSnapshotChanges();
        
        if (oldSnapshot is null)
            return diff;

        var currentEntries = oldSnapshot.GetAllEntries();
        var previousEntries = newSnapshot.GetAllEntries();
        var combined = currentEntries.Union(previousEntries);
        
        foreach (var entry in combined)
        {
            if (currentEntries.Contains(entry) && !previousEntries.Contains(entry))
                diff.AddedEntries.Add(entry);
            else if (previousEntries.Contains(entry) && !currentEntries.Contains(entry))
                diff.DeletedEntries.Add(entry);
            
            else if (entry is File)
            {
                var previousEntry = previousEntries.First(x => x.Equals(entry)) as File;
                var currentEntry = currentEntries.First(x => x.Equals(entry)) as File;
                currentEntry!.Version = previousEntry!.Version;

                if (FileHasChanged(currentEntry, oldSnapshot, folderPath))
                {
                    diff.ModifiedEntries.Add(currentEntry);
                    currentEntry!.Version++;
                }
            }
        }
        
        if (!diff.AddedEntries.Any() && !diff.DeletedEntries.Any() && !diff.ModifiedEntries.Any())
            logger.LogInformation("No changes detected in {FolderPath}", folderPath);
        else
        
        if (diff.AddedEntries.Any())
            logger.LogInformation("Items added in {FolderPath}: {AddedFiles}", folderPath, diff.AddedEntries.Count);
        if (diff.DeletedEntries.Any())
            logger.LogInformation("Items deleted in {FolderPath}: {DeletedFiles}", folderPath, diff.DeletedEntries.Count);
        if (diff.ModifiedEntries.Any())
            logger.LogInformation("Items modified in {FolderPath}: {ModifiedFiles}", folderPath, diff.ModifiedEntries.Count);

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