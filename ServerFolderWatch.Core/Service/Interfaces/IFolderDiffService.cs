using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service.Interfaces;

public interface IFolderDiffService
{
    FolderSnapshotDiff Diff(FolderSnapshot oldSnapshot, FolderSnapshot newSnapshot, string folderPath);
}