using System.IO.Abstractions;

namespace ServerFolderWatch.Core.Model;

public class FolderContents
{
    private FolderContents()
    {
    }
    
    public IList<Directory> Subfolders { get; private set; } = new List<Directory>();
    
    public IList<File> VersionedFiles { get; private set; } = new List<File>();

    public static FolderContents Empty { get; } = new();

    public static FolderContents FromFolder(string folderPath, IDirectory directory, IPath path)
    {
        return new FolderContents
        {
            Subfolders = directory.EnumerateDirectories(folderPath)
                .Select(path.GetDirectoryName).OfType<string>()
                .Select(x => new Directory(x)).ToList(),
            
            VersionedFiles = directory.EnumerateFiles(folderPath)
                .Select(path.GetFileName).OfType<string>()
                .Select(x => new File(x)).ToList()
        };
    }
}