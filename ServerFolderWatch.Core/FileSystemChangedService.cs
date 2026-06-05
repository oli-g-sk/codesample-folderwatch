using System.IO.Abstractions;
using System.Net;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core.Model;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Core;

public class FileSystemChangedService(IPath path, IDirectory directory, IFile file,
    IConfiguration configuration, ILoggerFactory loggerFactory)
    : IFileSystemChangeService
{
    private readonly ILogger<FileSystemChangedService> logger = loggerFactory.CreateLogger<FileSystemChangedService>();
    
    private string GetSidecarFilePath(string currentPath) => path.Combine(currentPath, configuration.SidecarFileName);
    
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
        
        bool wasAlreadyMonitored = file.Exists(GetSidecarFilePath(folderPath));

        if (wasAlreadyMonitored)
        {
            PreviousContents = GetContentsFromSidecarFile(folderPath);
        }
        else
        {
            logger.LogInformation("Setting up folder {FolderPath} for monitoring", folderPath);
            
            var sidecarFilePath = GetSidecarFilePath(folderPath);
            file.Create(sidecarFilePath).Close();

            foreach (var subfolder in directory.GetDirectories(folderPath))
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
        return FolderContents.FromFolder(folderPath, directory, path);
    }

    private FolderContents GetContentsFromSidecarFile(string folderPath)
    {
        try
        {
            var sidecarFileContents = JsonSerializer.Deserialize<FolderContents>
                (file.ReadAllText(GetSidecarFilePath(folderPath)));
            return sidecarFileContents ?? FolderContents.Empty;
        }
        catch (Exception ex)
        {
            // TODO log
        }
        
        return FolderContents.Empty;
    }
    
    private void SaveSidecarFile(string folderPath)
    {
        var json = JsonSerializer.Serialize(CurrentContents, new JsonSerializerOptions()
        {
            WriteIndented = true,
        });

        file.WriteAllText(GetSidecarFilePath(folderPath), json);       
    }
    
    private void DetectChanges(string folderPath)
    {
        if (PreviousContents is null)
            return;

        var current = CurrentContents.AllEntries;
        var previous = PreviousContents.AllEntries;
        var combined = current.Union(previous);
        
        foreach (var entry in combined)
        {
            if (current.Contains(entry) && !previous.Contains(entry))
                AddedEntries.Add(entry);
            else if (previous.Contains(entry) && !current.Contains(entry))
                DeletedEntries.Add(entry);
            
            else if (entry is File)
            {
                var previousEntry = previous.First(_ => true) as File;
                var currentEntry = current.First(_ => true) as File;
                currentEntry!.Version = previousEntry!.Version;

                if (FileHasChanged(currentEntry, folderPath))
                    currentEntry!.Version++;
                
                ModifiedEntries.Add(currentEntry);
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
        
        var fullPath = path.Combine(folderPath, fileEntry.Name);
        var modified = file.GetLastWriteTime(fullPath);

        // TODO other ways to detect changes? (size, MD5, etc.)
        return modified > PreviousContents.LastAnalyzed;
    }
}