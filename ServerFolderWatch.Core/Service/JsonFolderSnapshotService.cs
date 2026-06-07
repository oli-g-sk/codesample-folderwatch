using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;

namespace ServerFolderWatch.Core.Service;

// TODO composition over inheritance?
public class JsonFolderSnapshotService : BaseFolderSnapshotService
{
    private readonly IFileSystem fileSystem;
    private readonly IConfiguration configuration;
    private readonly ILogger<JsonFolderSnapshotService> logger;

    public JsonFolderSnapshotService(IBrowseService browseService, IFileSystem fileSystem, IConfiguration configuration, ILoggerFactory loggerFactory)
        : base(browseService, fileSystem, loggerFactory)
    {
        if (string.IsNullOrWhiteSpace(configuration.SidecarFileName))
            throw new ArgumentException("Invalid configuration: Sidecar file name is not set");
        if (fileSystem.Path.GetInvalidFileNameChars().Any(configuration.SidecarFileName.Contains))
            throw new ArgumentException("Invalid configuration: Sidecar file name cannot contain volume separator");
        
        this.fileSystem = fileSystem;
        this.configuration = configuration;
        logger = loggerFactory.CreateLogger<JsonFolderSnapshotService>();
    }
    
    public override bool IsFolderAlreadyMonitored(string folderPath)
    {
        return fileSystem.File.Exists(GetSidecarFilePath(folderPath));
    }
    
    public override FolderSnapshot LoadSnapshot(string folderPath)
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

    public override Task SaveSnapshot(string folderPath, Model.FolderSnapshot contents)
    {
        var json = JsonConvert.SerializeObject(contents, Formatting.Indented);
        var filePath = GetSidecarFilePath(folderPath);
        fileSystem.File.WriteAllText(filePath, json);
        return Task.CompletedTask;
    }

    protected override void InitializeFolderInternal(string folderPath)
    {
        var filePath = GetSidecarFilePath(folderPath);
        var stream = fileSystem.File.Create(filePath);
        stream?.Close();
    }

    private string GetSidecarFilePath(string currentPath) =>
        fileSystem.Path.Combine(currentPath, configuration.SidecarFileName);
}