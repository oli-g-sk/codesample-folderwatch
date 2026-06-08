namespace ServerFolderWatch.Core.Model;

/// <summary>
/// Represents the changes between two snapshots of the same folder
/// by listing the added, modified and deleted entries.
/// </summary>
public class FolderSnapshotChanges
{
    public List<BaseEntry> AddedEntries { get; } = new ();

    public List<BaseEntry> ModifiedEntries { get; } = new();

    public List<BaseEntry> DeletedEntries { get; } = new();
}