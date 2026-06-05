using System.Collections;

namespace ServerFolderWatch.Core;

public class FolderContents
{
    public IList<string> Subfolders { get; } = new List<string>();
    
    public IDictionary<string, int> VersionedFiles { get; } = new Dictionary<string, int>();
}