namespace ServerFolderWatch.Core.Model;

public class FolderSnapshot
{
    // TODO who should set this? persistence service when saving or diff when analyizing?
    public DateTime? LastAnalyzed { get; set; }
    
    public IList<Folder> Subfolders { get; internal set; } = new List<Folder>();
    
    public IList<File> VersionedFiles { get; internal set; } = new List<File>();
    
    public IList<BaseEntry> GetAllEntries() => Subfolders.OfType<BaseEntry>()
        .Concat(VersionedFiles)
        .Order()
        .ToList();

    // TODO remove - confusing - equals empty folder
    public static FolderSnapshot Empty { get; } = new();
}