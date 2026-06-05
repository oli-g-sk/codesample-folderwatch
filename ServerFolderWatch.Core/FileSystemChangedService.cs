using System.IO.Abstractions;
using System.Net;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core.Model;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Core;

public class FileSystemChangedService(IPath path, IDirectory directory, IFile file,
    IConfiguration configuration, ILogger<FileSystemChangedService> logger)
    : IFileSystemChangeService
{
    private readonly ILogger<FileSystemChangedService> logger = logger;
    
    private string SidecarFile => path.Combine(monitoredPath, configuration.SidecarFileName);
    
    private string monitoredPath;
    
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
        monitoredPath = folderPath; // TODO move to ctor
        
        bool wasAlreadyMonitored = file.Exists(SidecarFile);

        if (wasAlreadyMonitored)
        {
            PreviousContents = GetContentsFromSidecarFile();
        }
        else
        {
            file.Create(SidecarFile).Close();

            foreach (var subfolder in directory.GetDirectories(monitoredPath))
                await Analyze(subfolder);
        }

        CurrentContents = GetContentsFromFolder();
        CurrentContents.LastAnalyzed = DateTime.Now;
        
        DetectChanges();
        SaveSidecarFile();

        return wasAlreadyMonitored;
    }

    private FolderContents GetContentsFromFolder()
    {
        return FolderContents.FromFolder(monitoredPath, directory, path);
    }

    private FolderContents GetContentsFromSidecarFile()
    {
        try
        {
            var sidecarFileContents = JsonSerializer.Deserialize<FolderContents>(file.ReadAllText(SidecarFile));
            return sidecarFileContents ?? FolderContents.Empty;
        }
        catch (Exception ex)
        {
            // TODO log
        }
        
        return FolderContents.Empty;
    }
    
    private void SaveSidecarFile()
    {
        var json = JsonSerializer.Serialize(CurrentContents);
        file.WriteAllText(SidecarFile, json);       
    }
    
    private void DetectChanges()
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

                if (FileHasChanged(currentEntry))
                    currentEntry!.Version++;
                
                ModifiedEntries.Add(currentEntry);
            }
        }
    }

    private bool FileHasChanged(File fileEntry)
    {
        if (PreviousContents == null)
            return false;
        
        var fullPath = path.Combine(monitoredPath, fileEntry.Name);
        var modified = file.GetLastWriteTime(fullPath);

        // TODO other ways to detect changes? (size, MD5, etc.)
        return modified > PreviousContents.LastAnalyzed;
    }
}