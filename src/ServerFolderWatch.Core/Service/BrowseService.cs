using System.IO.Abstractions;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Core.Service;

public class BrowseService(IAppConfiguration configuration, IFileSystem fileSystem)
    : IBrowseService
{
    public bool IsPathValidAndBrowsable(string path)
    {
        // TODO disallow absolute paths
        // TODO check if path "tries to exit" the root public folder
        return fileSystem.Directory.Exists(path);
    }

    public bool CanGoToParent(string path)
    {
        var fullPath = Path.Combine(configuration.RootPublicPath, path);
        // TODO TEST
        return !fullPath.Equals(configuration.RootPublicPath);
    }
}