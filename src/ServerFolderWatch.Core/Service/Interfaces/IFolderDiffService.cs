using System.Net;
using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service.Interfaces;

/// <summary>
/// Provides the ability to compare two snapshots of the same folder.
/// TODO make original folder name a part of the snapshot
/// </summary>
public interface IFolderDiffService
{
    /// <summary>
    /// Compares two folder snapshots, returning the differences in two shapes.
    /// Use an <see cref="IFolderSnapshotService"/> to obtain the snapshots."
    /// </summary>
    /// <param name="oldSnapshot" />
    /// <param name="newSnapshot" />
    /// <param name="folderPath" />
    /// <param name="changes">
    /// Changes between the two snapshots in the form of three separate lists.
    /// </param>
    /// <returns>
    /// Changes between the two snapshots in the form of an unified list
    /// mapping file system entries to their diff operation.
    /// </returns>
    public Dictionary<FileSystemEntryBase, DiffOperation> Compare(FolderSnapshot? oldSnapshot, FolderSnapshot newSnapshot,
        string folderPath, out FolderSnapshotChanges changes);
    
    bool FileHasChanged(string fileName, string folderPath, DateTime? lastAnalyzed);
}
