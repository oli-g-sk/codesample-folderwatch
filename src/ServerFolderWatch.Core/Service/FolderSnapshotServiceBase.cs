using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Core.Service;

public abstract class FolderSnapshotServiceBase(
    IBrowseService browseService,
    IFolderDiffService folderDiffService,
    ILoggerFactory loggerFactory)
    : IFolderSnapshotService
{
    private readonly ILogger<FolderSnapshotServiceBase> logger
        = loggerFactory.CreateLogger<FolderSnapshotServiceBase>();

    public FolderSnapshot GetCurrentContents(string folderPath)
    {
        var currentState = new FolderSnapshot
        {
            Subfolders = browseService.GetSubfolders(folderPath),
            VersionedFiles = browseService.GetFiles(folderPath)
        };

        var previousState = LoadPersistedSnapshot(folderPath);
        
        foreach (var previousFile in previousState?.VersionedFiles ?? Enumerable.Empty<File>())
        {
            if (currentState.VersionedFiles.FirstOrDefault(x => x.Name == previousFile.Name) is { } currentFile)
                currentFile.Version = previousFile.Version;
        }
        
        return currentState;
    }

    public abstract bool IsFolderAlreadyMonitored(string folderPath);
    
    public abstract FolderSnapshot? LoadPersistedSnapshot(string folderPath);

    public async Task<FolderSnapshot> TakeSnapshot(string folderPath, bool recursive)
    {
        bool wasAlreadyMonitored = IsFolderAlreadyMonitored(folderPath);

        if (!wasAlreadyMonitored && !CanMonitorFolder(folderPath))
        {
            logger.LogWarning("Cannot take snapshot of folder: {folderPath}", folderPath);
            throw new InvalidOperationException($"Cannot take snapshot of folder: {folderPath}");           
        }

        var currentContents = GetCurrentContents(folderPath);
        
        var lastAnalyzed = wasAlreadyMonitored
            ? LoadPersistedSnapshot(folderPath)?.LastAnalyzed
            : null;

        foreach (var file in currentContents.VersionedFiles)
        {
            if (folderDiffService.FileHasChanged(file.Name, folderPath, lastAnalyzed))
                file.Version++;
        }
        
        currentContents.LastAnalyzed = DateTime.Now;
        await PersistSnapshot(folderPath, currentContents);
        
        logger.LogInformation(
            wasAlreadyMonitored
                ? "Persisted snapshot of folder: {folderPath}"
                : "Created initial snapshot of folder: {folderPath}",
            folderPath);

        if (!recursive)
        {
#if DEBUG
            // await Task.Delay(TimeSpan.FromSeconds(5));            
#endif
        }
        else
        {
            foreach (var subFolder in browseService.GetChildren(folderPath))
                await TakeSnapshot(subFolder, true);
        }

        return currentContents;            
    }

    protected abstract Task PersistSnapshot(string folderPath, FolderSnapshot snapshot);

    protected abstract bool CanMonitorFolder(string folderPath);
}
