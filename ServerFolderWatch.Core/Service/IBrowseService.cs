using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service;

public interface IBrowseService
{
    FolderContents ListContents(string folderPath);
}