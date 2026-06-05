namespace ServerFolderWatch.Core.Model;

public class FolderContents
{
    internal FolderContents()
    {
    }
    
    public DateTime LastAnalyzed { get; set; }
    
    public IList<Directory> Subfolders { get; internal set; } = new List<Directory>();
    
    public IList<File> VersionedFiles { get; internal set; } = new List<File>();
    
    public IList<FileSystemEntry> GetAllEntries() => Subfolders.OfType<FileSystemEntry>()
        .Concat(VersionedFiles)
        .ToList();

    // TODO remove - confusing - equals empty folder
    public static FolderContents Empty { get; } = new();
}