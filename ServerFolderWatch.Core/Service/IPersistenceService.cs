using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service;

public interface IPersistenceService
{
    public bool WasAlreadyMonitored(string folderPath);
    
    void Initialize(string folderPath);
    
    Task<FolderContents> Load(string folderPath);
    
    Task Save(string folderPath, FolderContents contents);
}