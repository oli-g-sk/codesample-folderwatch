using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service.Interfaces;

public interface IFolderDiffService
{
    FolderSnapshotDiff Analyze(FolderSnapshot oldSnapshot, FolderSnapshot newSnapshot,
        string folderPath, out FolderSnapshotChanges changes);
}