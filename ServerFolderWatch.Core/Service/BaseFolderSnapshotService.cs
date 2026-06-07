using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Core.Service;

public abstract class BaseFolderSnapshotService(IConfiguration configuration, IFileSystem fileSystem, ILoggerFactory loggerFactory)
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
            
            var initialSnapshot = GetCurrentContents(folderPath);
            initialSnapshot.LastAnalyzed = DateTime.Now;
            PersistSnapshot(folderPath, initialSnapshot).Wait();
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

    public abstract Task PersistSnapshot(string folderPath, FolderSnapshot contents);
    
    protected abstract void InitializeFolderInternal(string folderPath);
}