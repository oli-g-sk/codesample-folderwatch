namespace ServerFolderWatch.Core.Model;

public class File(string name) : BaseEntry(name)
{
    public int Version { get; internal set; } = 1;
}