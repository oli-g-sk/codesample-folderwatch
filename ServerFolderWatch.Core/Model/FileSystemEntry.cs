namespace ServerFolderWatch.Core.Model;

public abstract class FileSystemEntry(string name)
{
    public string Name { get; } = name;
}