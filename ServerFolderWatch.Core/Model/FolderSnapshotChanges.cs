namespace ServerFolderWatch.Core.Model;

/// <summary>
/// Represents the changes between two snapshots of the same folder
/// by listing the added, modified and deleted entries.
/// </summary>
public class FolderSnapshotChanges
{
    public List<FileSystemEntry> AddedEntries { get; } = new ();

    public List<FileSystemEntry> ModifiedEntries { get; } = new();

    public List<FileSystemEntry> DeletedEntries { get; } = new();
}