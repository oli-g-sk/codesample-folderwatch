using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;

namespace ServerFolderWatch.Core.Service;

public abstract class SidecarFileFolderSnapshotService : IFolderSnapshotService
{
    private readonly ILogger<SidecarFileFolderSnapshotService> logger;

    public SidecarFileFolderSnapshotService(IFileSystem fileSystem, IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        if (string.IsNullOrWhiteSpace(configuration.SidecarFileName))
            throw new ArgumentException("Invalid configuration: Sidecar file name is not set");
        if (fileSystem.Path.GetInvalidFileNameChars().Any(configuration.SidecarFileName.Contains))
            throw new ArgumentException("Invalid configuration: Sidecar file name cannot contain volume separator");
        
        this.fileSystem = fileSystem;
        this.configuration = configuration;
        logger = loggerFactory.CreateLogger<SidecarFileFolderSnapshotService>();
    }
    
    public bool IsFolderAlreadyMonitored(string folderPath)
    {
        return fileSystem.File.Exists(GetSidecarFilePath(folderPath));
    }

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
    
    public abstract FolderSnapshot LoadSnapshot(string folderPath);

    public abstract Task SaveSnapshot(string folderPath, FolderSnapshot contents);
    
    protected abstract void InitializeFolderInternal(string folderPath);
    
    private string GetSidecarFilePath(string currentPath) =>
        fileSystem.Path.Combine(currentPath, configuration.SidecarFileName);
    
    protected readonly IFileSystem fileSystem;
    protected readonly IConfiguration configuration;
}