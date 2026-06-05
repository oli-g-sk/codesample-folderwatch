using System.IO.Abstractions;
using System.Net;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerFolderWatch.Core.Model;
using File = ServerFolderWatch.Core.Model.File;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
        return FolderContents.FromFolder(folderPath, configuration, directory, path);
    }

    private FolderContents GetContentsFromSidecarFile(string folderPath)
    {
        try
        {
            var sidecarFileContents = JsonConvert.DeserializeObject<FolderContents>
                (file.ReadAllText(GetSidecarFilePath(folderPath)), new JsonSerializerSettings()
                {
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                });

            return sidecarFileContents ?? FolderContents.Empty;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading sidecar file in {FolderPath}: {Error}", folderPath, ex.Message);
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
        
        var fullPath = path.Combine(folderPath, fileEntry.Name);
        var modified = file.GetLastWriteTime(fullPath);

        // TODO other ways to detect changes? (size, MD5, etc.)
        return modified > PreviousContents.LastAnalyzed;
    }
}