using System.IO.Abstractions;
using System.Text.Json;
using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core;

public class FileSystemChangedService(IPath path, IDirectory directory, IFile file, IConfiguration configuration)
    : IFileSystemChangeService
{
    private string SidecarFile => path.Combine(monitoredPath, configuration.SidecarFileName);
    
    private string monitoredPath;
    
    public FolderContents? PreviousContents { get; private set; }
    
    public FolderContents CurrentContents { get; private set; }
    
    public List<string> AddedEntries { get; } = new();
    
    public List<(string, int)> ModifiedEntries { get; } = new();
    
    public List<string> DeletedEntries { get; } = new();

    
    public async Task<bool> Setup(string folderPath)
    {
        monitoredPath = folderPath; // TODO move to ctor
        
        bool wasAlreadyMonitored = file.Exists(SidecarFile);

        if (wasAlreadyMonitored)
        {
            PreviousContents = GetContentsFromSidecarFile();
        }
        else
        {
            file.Create(SidecarFile);

            foreach (var subfolder in directory.GetDirectories(monitoredPath))
                await Setup(subfolder);
        }

        CurrentContents = GetContentsFromFolder();
        DetectChanges();

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
    
    private void DetectChanges()
    {
        // TODO
    }
}