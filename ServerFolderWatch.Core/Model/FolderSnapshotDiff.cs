namespace ServerFolderWatch.Core.Model;

public class FolderSnapshotDiff
{
    public List<FileSystemEntry> AddedEntries { get; } = new ();

    public List<FileSystemEntry> ModifiedEntries { get; } = new();

    public List<FileSystemEntry> DeletedEntries { get; } = new();
}