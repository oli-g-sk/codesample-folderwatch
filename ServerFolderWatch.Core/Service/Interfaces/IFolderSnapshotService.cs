using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service.Interfaces;

public interface IFolderSnapshotService
{
    // TODO make this async
    void InitializeFolder(string folderPath, bool recursive);
    
    public bool IsFolderAlreadyMonitored(string folderPath);
    
    FolderSnapshot LoadSnapshot(string folderPath);
    
    Task SaveSnapshot(string folderPath, Model.FolderSnapshot contents);
}