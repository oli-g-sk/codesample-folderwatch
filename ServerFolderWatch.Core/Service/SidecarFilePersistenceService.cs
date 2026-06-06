using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;

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
    
    public void InitializeFolder(string folderPath, bool recursive)
    {
        if (IsFolderAlreadyMonitored(folderPath))
            throw new InvalidOperationException("Folder is already monitored");
        
        var filePath = GetSidecarFilePath(folderPath);
        var stream = fileSystem.File.Create(filePath);
        stream?.Close();

        if (recursive)
        {
            foreach (var subFolder in fileSystem.Directory.EnumerateDirectories(folderPath))
                InitializeFolder(subFolder, recursive);
        }
    }
    
    public FolderSnapshot LoadSnapshot(string folderPath)
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

    public Task SaveSnapshot(string folderPath, Model.FolderSnapshot contents)
    {
        var json = JsonConvert.SerializeObject(contents, Formatting.Indented);
        var filePath = GetSidecarFilePath(folderPath);
        fileSystem.File.WriteAllText(filePath, json);
        return Task.CompletedTask;
    }
}