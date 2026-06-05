using System.IO.Abstractions;
using System.Text.Json;

namespace ServerFolderWatch.Core;

public class FileSystemChangedService(IPath path, IDirectory directory, IFile file, IConfiguration configuration)
    : IFileSystemChangeService
{
    public async Task<bool> Setup(string monitoredPath)
    {
        string sidecarFile = path.Combine(monitoredPath, configuration.SidecarFileName);
        bool wasAlreadyMonitored = file.Exists(sidecarFile);

        if (!wasAlreadyMonitored)
        {
            file.Create(sidecarFile);
            
            foreach (var subfolder in directory.GetDirectories(monitoredPath))
                await Setup(subfolder);
        }
        else
            UpdateSidecarFile(monitoredPath);

        return wasAlreadyMonitored;
    }

    public List<string> GetAddedEntries()
    {
        throw new NotImplementedException();
    }

    public List<string> GetModifiedEntries()
    {
        throw new NotImplementedException();
    }

    public List<string> GetDeletedEntries()
    {
        throw new NotImplementedException();
    }
    
    private void UpdateSidecarFile(string monitoredPath)
    {
        string sidecarFile = path.Combine(monitoredPath, configuration.SidecarFileName);
        
        try
        {
            var previousContents = JsonSerializer.Deserialize<FolderContents>(file.ReadAllText(sidecarFile))
                ?? new FolderContents();

            SyncSubfolders(monitoredPath, previousContents);
        }
        catch (Exception ex)
        {
            // TODO log
        }
    }

    private void SyncSubfolders(string folderPath, FolderContents previousContents)
    {
        var currentSubfolders = directory.EnumerateDirectories(folderPath)
            .Select(Path.GetDirectoryName)
            .ToList();
        
        foreach (var subfolder in currentSubfolders)
        {
            if (string.IsNullOrEmpty(subfolder))
                continue;
            
            if (!previousContents.Subfolders.Contains(subfolder))
                previousContents.Subfolders.Add(subfolder);
        }

        foreach (var subfolder in previousContents.Subfolders)
        {
            if (!currentSubfolders.Contains(subfolder))
                previousContents.Subfolders.Remove(subfolder);
        }
    }
}