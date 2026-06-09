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

    public IEnumerable<string> GetChildren(string folderPath)
    {
        return fileSystem.Directory.EnumerateDirectories(folderPath);
    }
    
    public IList<Folder> GetSubfolders(string folderPath)
    {
        return fileSystem.Directory.EnumerateDirectories(folderPath)
            .Select(fileSystem.Path.GetFileName).OfType<string>()
            .Select(x => new Folder(x)).ToList();
    }

    public IList<File> GetFiles(string folderPath)
    {
        return fileSystem.Directory.EnumerateFiles(folderPath)
            .Select(fileSystem.Path.GetFileName).OfType<string>()
            .Where(x => !x.Equals(configuration.SidecarFileName))
            .Select(x => new File(x)).ToList();
    }
}