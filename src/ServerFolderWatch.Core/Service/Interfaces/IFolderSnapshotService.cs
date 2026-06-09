using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service.Interfaces;

public interface IFolderSnapshotService
{
    bool IsFolderAlreadyMonitored(string folderPath);
    
    /// <summary>
    /// Lists the current contents of a given folder
    /// (as seen by the file system) in the <see cref="FolderSnapshot"/> format.
    /// TODO not sure if here, or BrowseService is a better place for this
    /// </summary>
    FolderSnapshot GetCurrentContents(string folderPath);
    
    /// <summary>
    /// Loads the last persisted snapshot of the given folder.
    /// </summary>
    /// <param name="folderPath" />
    /// <returns></returns>
    FolderSnapshot LoadPersistedSnapshot(string folderPath);
    
    /// <summary>
    /// Takes and stores a snapshot of the folder in its current state.
    /// </summary>
    /// <param name="folderPath" />
    /// <param name="recursive">
    /// If true, traverses the subfolder tree, taking or overwriting snapshots.
    /// </param>
    Task TakeSnapshot(string folderPath, bool recursive);
}
