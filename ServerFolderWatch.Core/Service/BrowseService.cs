using System.IO.Abstractions;
using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service;

public class BrowseService(IConfiguration configuration, IFileSystem fileSystem)
    : IBrowseService
{
    public FolderContents ListContents(string folderPath)
    {
        return FolderContents.FromFolder(folderPath, configuration, fileSystem);
    }
}