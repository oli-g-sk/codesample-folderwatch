namespace ServerFolderWatch.Core.Model;

public class File(string name) : FileSystemEntryBase(name)
{
    public int Version { get; internal set; } = 1;
}