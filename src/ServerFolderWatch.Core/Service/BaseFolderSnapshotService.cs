using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Core.Service;

public abstract class BaseFolderSnapshotService(
    IAppConfiguration configuration,
    IFileSystem fileSystem,
    ILoggerFactory loggerFactory)
    : IFolderSnapshotService
{
    private readonly ILogger<BaseFolderSnapshotService> logger
        = loggerFactory.CreateLogger<BaseFolderSnapshotService>();

    public FolderSnapshot GetCurrentContents(string folderPath)
    {
        return new FolderSnapshot
        {
            Subfolders = fileSystem.Directory.EnumerateDirectories(folderPath)
                .Select(fileSystem.Path.GetFileName).OfType<string>()
                .Select(x => new Folder(x)).ToList(),
            
            VersionedFiles = fileSystem.Directory.EnumerateFiles(folderPath)
                .Select(fileSystem.Path.GetFileName).OfType<string>()
                .Where(x => !x.Equals(configuration.SidecarFileName))
                .Select(x => new File(x)).ToList()
        };
    }

    public abstract bool IsFolderAlreadyMonitored(string folderPath);
    
    public abstract FolderSnapshot LoadPersistedSnapshot(string folderPath);

    public async Task TakeSnapshot(string folderPath, bool recursive)
    {
        bool wasAlreadyMonitored = IsFolderAlreadyMonitored(folderPath);

        if (!wasAlreadyMonitored && !CanMonitorFolder(folderPath))
        {
            logger.LogWarning("Cannot take snapshot of folder: {folderPath}", folderPath);
            return;
        }

        await TakeSnapshotInternal(folderPath);
        logger.LogInformation(
            wasAlreadyMonitored
                ? "Persisted snapshot of folder: {folderPath}"
                : "Created initial snapshot of folder: {folderPath}",
            folderPath);

        if (!recursive)
            return;
        
        foreach (var subFolder in fileSystem.Directory.EnumerateDirectories(folderPath))
            await TakeSnapshot(subFolder, true);
    }
    
    protected abstract Task TakeSnapshotInternal(string folderPath);

    protected abstract bool CanMonitorFolder(string folderPath);
}
