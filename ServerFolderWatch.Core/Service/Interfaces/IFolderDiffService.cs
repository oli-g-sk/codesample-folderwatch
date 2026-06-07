using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service.Interfaces;

/// <summary>
/// Provides the ability to compare two snapshots of the same folder.
/// TODO make folder name a part of the snapshot, so we don't compare Foo and Bar?
/// </summary>
public interface IFolderDiffService
{
    FolderSnapshotDiff Compare(FolderSnapshot oldSnapshot, FolderSnapshot newSnapshot,
        string folderPath, out FolderSnapshotChanges changes);
}