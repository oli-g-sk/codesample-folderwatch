using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;

namespace ServerFolderWatch.Core.Service;

public abstract class BaseFolderSnapshotService(IFileSystem fileSystem, ILoggerFactory loggerFactory)
    : IFolderSnapshotService
{
    private readonly ILogger<BaseFolderSnapshotService> logger
        = loggerFactory.CreateLogger<BaseFolderSnapshotService>();

    public bool InitializeFolder(string folderPath, bool recursive)
    {
        bool wasInitialzed = false;
        
        if (!IsFolderAlreadyMonitored(folderPath))
        {
            InitializeFolderInternal(folderPath);
            wasInitialzed = true;
            SaveSnapshot(folderPath, FolderSnapshot.Empty).Wait();
        }

        if (recursive)
        {
            foreach (var subFolder in fileSystem.Directory.EnumerateDirectories(folderPath))
                wasInitialzed = wasInitialzed || InitializeFolder(subFolder, recursive);
        }

        return wasInitialzed;
    }

    public abstract bool IsFolderAlreadyMonitored(string folderPath);
    
    public abstract FolderSnapshot LoadSnapshot(string folderPath);

    public abstract Task SaveSnapshot(string folderPath, FolderSnapshot contents);
    
    protected abstract void InitializeFolderInternal(string folderPath);
}