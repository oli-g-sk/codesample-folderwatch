using System.Collections;
using System.IO.Abstractions;

namespace ServerFolderWatch.Core;

public class FolderContents
{
    private FolderContents()
    {
    }
    
    public IList<string> Subfolders { get; private set; } = new List<string>();
    
    public IDictionary<string, int> VersionedFiles { get; private set; } = new Dictionary<string, int>();

    public static FolderContents Empty { get; } = new();

    public static FolderContents FromFolder(string folderPath, IDirectory directory, IPath path)
    {
        var subfolders = directory.EnumerateDirectories(folderPath)
            .Select(path.GetDirectoryName).OfType<string>();

        var files = directory.EnumerateFiles(folderPath)
            .Select(path.GetFileName).OfType<string>();
        
        var versionedFiles = files.ToDictionary(x => x, x => 1);

        return new FolderContents
        {
            Subfolders = subfolders.ToList(),
            VersionedFiles = versionedFiles
        };
    }
}