using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service;

public class SidecarFilePersistenceService(IFileSystem fileSystem, IConfiguration configuration, ILoggerFactory loggerFactory)
    : IPersistenceService
{
    private readonly ILogger<SidecarFilePersistenceService> logger
        = loggerFactory.CreateLogger<SidecarFilePersistenceService>();
    
    private string GetSidecarFilePath(string currentPath) => fileSystem.Path.Combine(currentPath, configuration.SidecarFileName);
    
    public bool IsFolderAlreadyMonitored(string folderPath)
    {
        return fileSystem.File.Exists(GetSidecarFilePath(folderPath));
    }
    
    public void InitializeFolder(string folderPath)
    {
        var filePath = GetSidecarFilePath(folderPath);
        fileSystem.File.Create(filePath).Close();
    }
    
    public Task<FolderContents> LoadSnapshot(string folderPath)
    {
        try
        {
            var sidecarFileContents = JsonConvert.DeserializeObject<FolderContents>
            (fileSystem.File.ReadAllText(GetSidecarFilePath(folderPath)), new JsonSerializerSettings()
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            });

            return Task.FromResult(sidecarFileContents ?? FolderContents.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading sidecar file in {FolderPath}: {Error}", folderPath, ex.Message);
        }
        
        return Task.FromResult(FolderContents.Empty);
    }

    public Task SaveSnapshot(string folderPath, FolderContents contents)
    {
        var json = JsonConvert.SerializeObject(contents, Formatting.Indented);
        var filePath = GetSidecarFilePath(folderPath);
        fileSystem.File.WriteAllText(filePath, json);
        return Task.CompletedTask;
    }
}