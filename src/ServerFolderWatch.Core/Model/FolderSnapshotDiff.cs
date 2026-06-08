namespace ServerFolderWatch.Core.Model;

/// <summary>
/// Represents the difference between two snapshots of the same folder
/// as a single list containing both past and present items, along with
/// the diff operation for each.
/// </summary>
/// TODO is this class needed, or can the service return list of tuples directly?
public class FolderSnapshotDiff
{
    public List<(BaseEntry FileSystemEntry, DiffOperation Operation)> Entries { get; } = new();
}