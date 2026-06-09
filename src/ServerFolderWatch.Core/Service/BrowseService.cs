using System.IO.Abstractions;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Core.Service;

public class BrowseService(IAppConfiguration configuration, IFileSystem fileSystem)
    : IBrowseService
{
    public string GetFileSystemPath(string? folderPath)
    {
        return NormalizePath(folderPath);
    }
    
    public bool FolderExists(string folderPath)
    {
        folderPath = NormalizePath(folderPath);
        
        return fileSystem.Directory.Exists(folderPath);
    }
    
    public bool CanReadFolderContents(string folderPath)
    {
        folderPath = NormalizePath(folderPath);
        
        try
        {
            _ = fileSystem.Directory.EnumerateFiles(folderPath);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    public bool CanWriteToFolder(string folderPath)
    {
        folderPath = NormalizePath(folderPath);
        
        try
        {
            string filePath = fileSystem.Path.Combine(folderPath, ".test");
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

    public bool CanGoToParent(string folderPath)
    {
        var fullPath = NormalizePath(folderPath);
        var rootPath = GetRootPublicFileSystemPath();
        return !fileSystem.Path.TrimEndingDirectorySeparator(fullPath)
            .Equals(fileSystem.Path.TrimEndingDirectorySeparator(rootPath), StringComparison.OrdinalIgnoreCase);
    }

    public bool CanBrowsePath(string folderPath)
    {
        folderPath = NormalizePath(folderPath);
        
        return fileSystem.Directory.Exists(folderPath);
    }

    public IEnumerable<string> GetChildren(string folderPath)
    {
        folderPath = NormalizePath(folderPath);
        
        return fileSystem.Directory.EnumerateDirectories(folderPath);
    }
    
    public IList<Folder> GetSubfolders(string folderPath)
    {
        folderPath = NormalizePath(folderPath);
        
        return fileSystem.Directory.EnumerateDirectories(folderPath)
            .Select(fileSystem.Path.GetFileName).OfType<string>()
            .Select(x => new Folder(x)).ToList();
    }

    public IList<File> GetFiles(string folderPath)
    {
        folderPath = NormalizePath(folderPath);
        
        return fileSystem.Directory.EnumerateFiles(folderPath)
            .Select(fileSystem.Path.GetFileName).OfType<string>()
            .Where(x => !x.Equals(configuration.SidecarFileName))
            .Select(x => new File(x)).ToList();
    }

    // TODO move to ASP.NET middleware?
    private string NormalizePath(string? folderPath)
    {
        string fullRootFolderPath = GetRootPublicFileSystemPath();
        folderPath = folderPath?.Replace('/', fileSystem.Path.DirectorySeparatorChar);
        
        if (string.IsNullOrWhiteSpace(folderPath))
            folderPath = fullRootFolderPath;
        else if (!fileSystem.Path.IsPathFullyQualified(folderPath))
            folderPath = fileSystem.Path.GetFullPath(
                fileSystem.Path.Combine(fullRootFolderPath, folderPath));
        else
            folderPath = fileSystem.Path.GetFullPath(folderPath);
        
        if (!IsUnderRoot(folderPath, fullRootFolderPath))
            throw new UnauthorizedAccessException("Path must be under the public folder. "
                + "Path: " + folderPath + ", Root: " + configuration.RootPublicPath);

        return folderPath;
    }

    private string GetRootPublicFileSystemPath()
    {
        return fileSystem.Path.IsPathFullyQualified(configuration.RootPublicPath)
            ? fileSystem.Path.GetFullPath(configuration.RootPublicPath)
            : fileSystem.Path.GetFullPath(configuration.RootPublicPath, AppContext.BaseDirectory);
    }

    private bool IsUnderRoot(string folderPath, string rootPath)
    {
        folderPath = fileSystem.Path.TrimEndingDirectorySeparator(folderPath);
        rootPath = fileSystem.Path.TrimEndingDirectorySeparator(rootPath);

        return folderPath.Equals(rootPath, StringComparison.OrdinalIgnoreCase)
            || folderPath.StartsWith(rootPath + fileSystem.Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase);
    }
}
