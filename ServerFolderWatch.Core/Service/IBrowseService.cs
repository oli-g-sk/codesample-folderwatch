using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service;

public interface IBrowseService
{
    bool IsPathValid(string path);
    
    FolderContents ListContents(string folderPath);
}