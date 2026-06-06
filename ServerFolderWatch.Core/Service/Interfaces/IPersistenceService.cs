using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service.Interfaces;

public interface IPersistenceService
{
    void InitializeFolder(string folderPath, bool recursive);
    
    public bool IsFolderAlreadyMonitored(string folderPath);
    
    Task<FolderSnapshot> LoadSnapshot(string folderPath);
    
    Task SaveSnapshot(string folderPath, Model.FolderSnapshot contents);
}