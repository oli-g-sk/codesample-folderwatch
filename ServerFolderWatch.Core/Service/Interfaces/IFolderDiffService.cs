using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service.Interfaces;

public interface IFolderDiffService
{
    FolderSnapshotDiff Compare(FolderSnapshot oldSnapshot, FolderSnapshot newSnapshot,
        string folderPath, out FolderSnapshotChanges changes);
}