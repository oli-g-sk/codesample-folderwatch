using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service;

public class JsonFileSnapshotService : SidecarFileFolderSnapshotService
{
    private readonly IFileSystem fileSystem;
    private readonly IConfiguration configuration;
    private readonly ILogger<JsonFileSnapshotService> logger;

    public JsonFileSnapshotService(IFileSystem fileSystem, IConfiguration configuration, ILoggerFactory loggerFactory)
        : base(fileSystem, configuration, loggerFactory)
    {
        this.fileSystem = fileSystem;
        this.configuration = configuration;
        logger = loggerFactory.CreateLogger<JsonFileSnapshotService>();
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