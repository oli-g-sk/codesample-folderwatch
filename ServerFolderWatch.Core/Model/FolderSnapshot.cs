namespace ServerFolderWatch.Core.Model;

public class FolderSnapshot
{
    public DateTime LastAnalyzed { get; set; }
    
    public IList<Directory> Subfolders { get; internal set; } = new List<Directory>();
    
    public IList<File> VersionedFiles { get; internal set; } = new List<File>();
    
    public IList<FileSystemEntry> GetAllEntries() => Subfolders.OfType<FileSystemEntry>()
        .Concat(VersionedFiles)
        .ToList();

    // TODO remove - confusing - equals empty folder
    public static FolderSnapshot Empty { get; } = new();
}