using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service;

public interface IPersistenceService
{
    void InitializeFolder(string folderPath);
    
    public bool IsFolderAlreadyMonitored(string folderPath);
    
    Task<FolderContents> LoadSnapshot(string folderPath);
    
    Task SaveSnapshot(string folderPath, FolderContents contents);
}