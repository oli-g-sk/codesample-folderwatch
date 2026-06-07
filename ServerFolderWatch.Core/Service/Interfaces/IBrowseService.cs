using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service.Interfaces;

/// <summary>
/// Allows browsing the file system, listing folders
/// contents in the <see cref="FolderSnapshot"/> format.
/// </summary>
public interface IBrowseService
{
    bool IsPathValidAndBrowsable(string path);
    
    bool CanGoToParent(string path);
    
    /// <summary>
    /// Lists the current contents of a given folder
    /// (as seen by the file system) in the <see cref="FolderSnapshot"/> format."/>
    /// </summary>
    FolderSnapshot ListContents(string folderPath);
}