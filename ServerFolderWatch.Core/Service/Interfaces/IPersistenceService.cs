namespace ServerFolderWatch.Core.Service.Interfaces;

public interface IPersistenceService
{
    void InitializeFolder(string folderPath);
    
    public bool IsFolderAlreadyMonitored(string folderPath);
    
    Task<Model.FolderSnapshot> LoadSnapshot(string folderPath);
    
    Task SaveSnapshot(string folderPath, Model.FolderSnapshot contents);
}