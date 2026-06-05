namespace ServerFolderWatch.Server.DTOs;

public enum FileSystemEntityDiffOperation
{
    Unchanged,
    Added,
    Removed,
    Modified,
    Renamed // TODO add support for rename
}