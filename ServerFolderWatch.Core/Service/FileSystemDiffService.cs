using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core.Model;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Core.Service;

public class FileSystemDiffService(IFileSystem fileSystem,
    IPersistenceService persistenceService,
    IConfiguration configuration, ILoggerFactory loggerFactory)
    : IFileSystemDiffService
{
    private readonly ILogger<FileSystemDiffService> logger = loggerFactory.CreateLogger<FileSystemDiffService>();
    
    public FolderContents? PreviousContents { get; private set; }
    
    public FolderContents CurrentContents { get; private set; }
    
    // TODO add tests
    public List<FileSystemEntry> AddedEntries { get; } = new();
    
    // TODO add tests
    public List<FileSystemEntry> ModifiedEntries { get; } = new();
    
    // TODO add tests
    public List<FileSystemEntry> DeletedEntries { get; } = new();
    
    public async Task<bool> Analyze(string folderPath)
    {
        logger.LogTrace("Analyzing {FolderPath}", folderPath);
        
        bool wasAlreadyMonitored = persistenceService.IsFolderAlreadyMonitored(folderPath);

        if (wasAlreadyMonitored)
        {
            PreviousContents = GetContentsFromSidecarFile(folderPath);
        }
        else
        {
            logger.LogInformation("Setting up folder {FolderPath} for monitoring", folderPath);

            persistenceService.InitializeFolder(folderPath);
            
            foreach (var subfolder in fileSystem.Directory.GetDirectories(folderPath))
                await Analyze(subfolder);
        }

        CurrentContents = GetContentsFromFolder(folderPath);
        CurrentContents.LastAnalyzed = DateTime.Now;
        
        DetectChanges(folderPath);
        SaveSidecarFile(folderPath);

        return wasAlreadyMonitored;
    }

    private FolderContents GetContentsFromFolder(string folderPath)
    {
        return FolderContents.FromFolder(folderPath, configuration, fileSystem);
    }

    private FolderContents GetContentsFromSidecarFile(string folderPath)
    {
        return persistenceService.LoadSnapshot(folderPath).Result;
    }
    
    private void SaveSidecarFile(string folderPath)
    {
        persistenceService.SaveSnapshot(folderPath, CurrentContents);
    }
    
    private void DetectChanges(string folderPath)
    {
        if (PreviousContents is null)
            return;

        var currentEntries = CurrentContents.GetAllEntries();
        var previousEntries = PreviousContents.GetAllEntries();
        var combined = currentEntries.Union(previousEntries);
        
        foreach (var entry in combined)
        {
            if (currentEntries.Contains(entry) && !previousEntries.Contains(entry))
                AddedEntries.Add(entry);
            else if (previousEntries.Contains(entry) && !currentEntries.Contains(entry))
                DeletedEntries.Add(entry);
            
            else if (entry is File)
            {
                var previousEntry = previousEntries.First(x => x.Equals(entry)) as File;
                var currentEntry = currentEntries.First(x => x.Equals(entry)) as File;
                currentEntry!.Version = previousEntry!.Version;

                if (FileHasChanged(currentEntry, folderPath))
                {
                    ModifiedEntries.Add(currentEntry);
                    currentEntry!.Version++;
                }
            }
        }
        
        if (!AddedEntries.Any() && !DeletedEntries.Any() && !ModifiedEntries.Any())
            logger.LogInformation("No changes detected in {FolderPath}", folderPath);
        else
        
        if (AddedEntries.Any())
            logger.LogInformation("Items added in {FolderPath}: {AddedFiles}", folderPath, AddedEntries.Count);
        if (DeletedEntries.Any())
            logger.LogInformation("Items deleted in {FolderPath}: {DeletedFiles}", folderPath, DeletedEntries.Count);
        if (ModifiedEntries.Any())
            logger.LogInformation("Items modified in {FolderPath}: {ModifiedFiles}", folderPath, ModifiedEntries.Count);       
    }

    private bool FileHasChanged(File fileEntry, string folderPath)
    {
        if (PreviousContents == null)
            return false;
        
        var fullPath = fileSystem.Path.Combine(folderPath, fileEntry.Name);
        var modified = fileSystem.File.GetLastWriteTime(fullPath);

        // TODO other ways to detect changes? (size, MD5, etc.)
        return modified > PreviousContents.LastAnalyzed;
    }
}