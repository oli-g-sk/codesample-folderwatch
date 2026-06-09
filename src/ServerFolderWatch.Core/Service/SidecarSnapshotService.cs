using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;

namespace ServerFolderWatch.Core.Service;

// TODO composition over inheritance?
public class SidecarSnapshotService : BaseFolderSnapshotService
{
    private readonly IBrowseService browseService;
    private readonly IFileSystem fileSystem;
    private readonly IAppConfiguration configuration;
    private readonly ILogger<SidecarSnapshotService> logger;

    public SidecarSnapshotService(
        IBrowseService browseService,
        IAppConfiguration configuration,
        IFileSystem fileSystem,
        ILoggerFactory loggerFactory)
        : base(browseService, loggerFactory)
    {
        if (string.IsNullOrWhiteSpace(configuration.SidecarFileName))
            throw new ArgumentException("Invalid configuration: Sidecar file name is not set");
        if (fileSystem.Path.GetInvalidFileNameChars().Any(configuration.SidecarFileName.Contains))
            throw new ArgumentException("Invalid configuration: Sidecar file name cannot contain volume separator");

        this.browseService = browseService;
        this.fileSystem = fileSystem;
        this.configuration = configuration;
        logger = loggerFactory.CreateLogger<SidecarSnapshotService>();
    }
    
    public override bool IsFolderAlreadyMonitored(string folderPath)
    {
        return fileSystem.File.Exists(GetSidecarFilePath(folderPath));
    }
    
    public override FolderSnapshot LoadPersistedSnapshot(string folderPath)
    {
        try
        {
            var sidecarFileContents = JsonConvert.DeserializeObject<Model.FolderSnapshot>
            (fileSystem.File.ReadAllText(GetSidecarFilePath(folderPath)), new JsonSerializerSettings()
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            });

            return sidecarFileContents ?? FolderSnapshot.Empty;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading sidecar file in {FolderPath}: {Error}", folderPath, ex.Message);
        }
        
        // TODO return null
        return FolderSnapshot.Empty;
    }

    protected override bool CanMonitorFolder(string folderPath)
    {
        return browseService.CanWriteToFolder(folderPath);
    }

    protected override Task PersistSnapshot(string folderPath, FolderSnapshot snapshot)
    {
        var filePath = GetSidecarFilePath(folderPath);
        
        try
        {
            var json = JsonConvert.SerializeObject(snapshot, Formatting.Indented);
            fileSystem.File.WriteAllText(filePath, json);
            logger.LogDebug("Persisted snapshot of folder {FilePath}", folderPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error writing sidecar file in {FolderPath}: {Error}", folderPath, ex.Message);
        }

        return Task.CompletedTask;
    }

    private string GetSidecarFilePath(string currentPath) =>
        fileSystem.Path.Combine(currentPath, configuration.SidecarFileName);
}
