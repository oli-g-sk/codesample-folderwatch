using System.IO.Abstractions;
using Newtonsoft.Json;

namespace ServerFolderWatch.Core.Model;

public class FolderContents
{
    private FolderContents()
    {
    }
    
    public DateTime LastAnalyzed { get; set; }
    
    public IList<Directory> Subfolders { get; private set; } = new List<Directory>();
    
    public IList<File> VersionedFiles { get; private set; } = new List<File>();
    
    public IList<FileSystemEntry> GetAllEntries() => Subfolders.OfType<FileSystemEntry>()
        .Concat(VersionedFiles)
        .DistinctBy(x => x.Name)
        .OrderBy(x => x.Name)
        .ToList();

    // TODO remove - confusing - equals empty folder
    public static FolderContents Empty { get; } = new();

    public static FolderContents FromFolder(string folderPath, IConfiguration configuration,
        IFileSystem fileSystem)
    {
        return new FolderContents
        {
            Subfolders = fileSystem.Directory.EnumerateDirectories(folderPath)
                .Select(fileSystem.Path.GetDirectoryName).OfType<string>()
                .Select(x => new Directory(x)).ToList(),
            
            VersionedFiles = fileSystem.Directory.EnumerateFiles(folderPath)
                .Select(fileSystem.Path.GetFileName).OfType<string>()
                .Where(x => !x.Equals(configuration.SidecarFileName))
                .Select(x => new File(x)).ToList()
        };
    }
}