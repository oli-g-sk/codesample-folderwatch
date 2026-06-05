using System.IO.Abstractions;

namespace ServerFolderWatch.Core;

public class FileSystemChangedService(IPath path, IDirectory directory, IFile file, IConfiguration configuration)
    : IFileSystemChangeService
{
    private readonly IPath path = path;
    private readonly IDirectory directory = directory;
    private readonly IFile file = file;
    private readonly IConfiguration configuration = configuration;

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
    
    private bool IsSetupInFolder(string monitoredPath)
    {
        return file.Exists(path.Combine(monitoredPath, configuration.SidecarFileName));
    }

    private async Task SetupRecursively(string rootPath)
    {
        // TODO
        await Task.CompletedTask;
    }
}