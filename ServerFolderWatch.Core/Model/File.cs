namespace ServerFolderWatch.Core.Model;

public class File(string name) : FileSystemEntry(name)
{
    public int Version { get; internal set; } = 1;
}