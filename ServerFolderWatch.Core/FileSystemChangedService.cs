using System.IO.Abstractions;
using System.Text.Json;

namespace ServerFolderWatch.Core;

public class FileSystemChangedService(IPath path, IDirectory directory, IFile file, IConfiguration configuration)
    : IFileSystemChangeService
{
    private string monitoredPath;
    private FolderContents previousContents;
    private FolderContents currentContents;
    
    private string SidecarFile => path.Combine(monitoredPath, configuration.SidecarFileName);
    
    public async Task<bool> Setup(string folderPath)
    {
        monitoredPath = folderPath; // TODO move to ctor
        
        bool wasAlreadyMonitored = file.Exists(SidecarFile);

        if (wasAlreadyMonitored)
        {
            previousContents = GetContentsFromSidecarFile();
        }
        else
        {
            file.Create(SidecarFile);

            foreach (var subfolder in directory.GetDirectories(monitoredPath))
                await Setup(subfolder);
        }

        currentContents = GetContentsFromFolder();
        DetectChanges();

        return wasAlreadyMonitored;
    }

    public List<string> AddedEntries { get; private set; }
    
    public List<(string, int)> ModifiedEntries { get; private set; }
    
    public List<string> DeletedEntries { get; private set; }

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
        
    }
}