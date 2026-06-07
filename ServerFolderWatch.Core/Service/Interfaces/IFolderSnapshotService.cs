using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service.Interfaces;

public interface IFolderSnapshotService
{
    // TODO make this async
    bool InitializeFolder(string folderPath, bool recursive);
    
    bool IsFolderAlreadyMonitored(string folderPath);
    
    /// <summary>
    /// Lists the current contents of a given folder
    /// (as seen by the file system) in the <see cref="FolderSnapshot"/> format.
    /// TODO not sure if here, or BrowseService is a better place for this
    /// </summary>
    FolderSnapshot GetCurrentContents(string folderPath);
    
    FolderSnapshot LoadPersistedSnapshot(string folderPath);
    
    Task PersistSnapshot(string folderPath, Model.FolderSnapshot contents);
}