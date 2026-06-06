using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service.Interfaces;

public interface IBrowseService
{
    bool IsPathValidAndBrowsable(string path);
    
    bool CanGoToParent(string path);
    
    FolderSnapshot ListContents(string folderPath);
}