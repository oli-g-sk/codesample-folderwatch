namespace ServerFolderWatch.Core.Model;

public enum DiffOperation
{
    Unchanged,
    Added,
    Removed,
    Modified,
    Renamed // TODO add support for rename
}