using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Core.Service;

public class MainService(IFileSystem fileSystem,
    IBrowseService browseService,
    IPersistenceService persistenceService,
    ILoggerFactory loggerFactory)
    : IMainService
{
    private readonly ILogger<MainService> logger = loggerFactory.CreateLogger<MainService>();
    
    public FolderSnapshot? PreviousContents { get; private set; }
    
    public FolderSnapshot CurrentContents { get; private set; }
    
    public IList<FileSystemEntry> CurrentEntries => CurrentContents.GetAllEntries().Order().ToList();
    
    public IList<FileSystemEntry> AllEntries => CurrentContents.GetAllEntries()
        .Union(PreviousContents?.GetAllEntries() ?? Enumerable.Empty<FileSystemEntry>())
        .ToList();
    
    // TODO add tests
    public ICollection<FileSystemEntry> AddedEntries { get; } = new List<FileSystemEntry>();
    
    // TODO add tests
    public ICollection<FileSystemEntry> ModifiedEntries { get; } = new List<FileSystemEntry>();
    
    // TODO add tests
    public ICollection<FileSystemEntry> DeletedEntries { get; } = new List<FileSystemEntry>();
    
    public async Task<bool> Analyze(string folderPath)
    {
        AddedEntries.Clear();
        ModifiedEntries.Clear();
        DeletedEntries.Clear();
        
        logger.LogTrace("Analyzing {FolderPath}", folderPath);
        
        bool initializeNeeded = !persistenceService.IsFolderAlreadyMonitored(folderPath);

        if (initializeNeeded)
        {
            logger.LogInformation("Setting up folder {FolderPath} for monitoring", folderPath);

            persistenceService.InitializeFolder(folderPath);
            
            foreach (var subfolder in fileSystem.Directory.GetDirectories(folderPath))
                await Analyze(subfolder);
        }
        else
        {
            PreviousContents = GetContentsFromSidecarFile(folderPath);
        }

        CurrentContents = GetContentsFromFolder(folderPath);
        CurrentContents.LastAnalyzed = DateTime.Now;
        
        DetectChanges(folderPath);
        SaveSidecarFile(folderPath);

        return initializeNeeded;
    }

    private FolderSnapshot GetContentsFromFolder(string folderPath)
    {
        return browseService.ListContents(folderPath);
    }

    private FolderSnapshot GetContentsFromSidecarFile(string folderPath)
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