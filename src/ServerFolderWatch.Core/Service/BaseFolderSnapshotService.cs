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

    public bool InitializeFolder(string folderPath, bool recursive)
    {
        bool wasInitialzed = false;
        
        if (!IsFolderAlreadyMonitored(folderPath))
        {
            if (!CanMonitorFolder(folderPath))
            {
                logger.LogWarning("Cannot set up folder for monitoring: {folderPath}", folderPath);
                
                // early abort; assume if we cannot monitor this folder,
                // there's no point continuing the recursion
                return false;
            }

            else
            {
                wasInitialzed = InitializeFolderInternal(folderPath);

                if (wasInitialzed)
                {
                    var initialSnapshot = GetCurrentContents(folderPath);
                    initialSnapshot.LastAnalyzed = DateTime.Now;
                    TakeSnapshot(folderPath).Wait();
                    logger.LogInformation("Created initial snapshot of folder: {folderPath}", folderPath);
                }
            }
        }

        if (recursive)
        {
            foreach (var subFolder in fileSystem.Directory.EnumerateDirectories(folderPath))
                wasInitialzed = InitializeFolder(subFolder, recursive) || wasInitialzed;
        }

        return wasInitialzed;
    }
    
    
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

    public abstract Task TakeSnapshot(string folderPath);
    
    protected abstract bool InitializeFolderInternal(string folderPath);
    
    protected abstract bool CanMonitorFolder(string folderPath);
}