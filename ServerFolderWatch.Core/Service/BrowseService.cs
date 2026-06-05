using System.IO.Abstractions;
using ServerFolderWatch.Core.Model;
using Directory = ServerFolderWatch.Core.Model.Directory;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Core.Service;

public class BrowseService(IConfiguration configuration, IFileSystem fileSystem)
    : IBrowseService
{
    public FolderContents ListContents(string folderPath)
    {
        return new FolderContents
        {
            Subfolders = fileSystem.Directory.EnumerateDirectories(folderPath)
                .Select(fileSystem.Path.GetFileName).OfType<string>()
                .Select(x => new Directory(x)).ToList(),
            
            VersionedFiles = fileSystem.Directory.EnumerateFiles(folderPath)
                .Select(fileSystem.Path.GetFileName).OfType<string>()
                .Where(x => !x.Equals(configuration.SidecarFileName))
                .Select(x => new File(x)).ToList()
        };
    }
}