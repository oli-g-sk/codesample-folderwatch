using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;

namespace ServerFolderWatch.Core.Service;

public abstract class BaseFolderSnapshotService(
    IBrowseService browseService,
    ILoggerFactory loggerFactory)
    : IFolderSnapshotService
{
    private readonly ILogger<BaseFolderSnapshotService> logger
        = loggerFactory.CreateLogger<BaseFolderSnapshotService>();

    public FolderSnapshot GetCurrentContents(string folderPath)
    {
        return new FolderSnapshot
        {
            Subfolders = browseService.GetSubfolders(folderPath),
            VersionedFiles = browseService.GetFiles(folderPath)
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

        var currentContents = GetCurrentContents(folderPath);
        currentContents.LastAnalyzed = DateTime.Now; // TODO test
        await PersistSnapshot(folderPath, currentContents);
        
        logger.LogInformation(
            wasAlreadyMonitored
                ? "Persisted snapshot of folder: {folderPath}"
                : "Created initial snapshot of folder: {folderPath}",
            folderPath);

        if (!recursive)
            return;
        
        foreach (var subFolder in browseService.GetChildren(folderPath))
            await TakeSnapshot(subFolder, true);
    }

    protected abstract Task PersistSnapshot(string folderPath, FolderSnapshot snapshot);

    protected abstract bool CanMonitorFolder(string folderPath);
}
