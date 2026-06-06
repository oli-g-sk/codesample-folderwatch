using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service;

public interface IBrowseService
{
    bool IsPathValidAndBrowsable(string path);
    
    FolderSnapshot ListContents(string folderPath);
}