using System.IO.Abstractions;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Core.Service;

public class BrowseService(IAppConfiguration configuration, IFileSystem fileSystem)
    : IBrowseService
{
    public bool CanReadFolderContents(string path)
    {
        try
        {
            _ = fileSystem.Directory.EnumerateFiles(path);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    public bool CanWriteToFolder(string path)
    {
        try
        {
            string filePath = fileSystem.Path.Combine(path, ".test");
            var handle = fileSystem.File.Create(filePath);
            handle?.Close();
            fileSystem.File.Delete(filePath);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }

    public bool CanGoToParent(string path)
    {
        var fullPath = Path.Combine(configuration.RootPublicPath, path);
        // TODO TEST
        return !fullPath.Equals(configuration.RootPublicPath);
    }

    public bool CanBrowsePath(string path)
    {
        return fileSystem.Directory.Exists(path);
    }
}