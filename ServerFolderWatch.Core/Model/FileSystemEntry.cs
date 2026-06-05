namespace ServerFolderWatch.Core.Model;

public abstract class FileSystemEntry(string name)
{
    public string Name { get; } = name;

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((FileSystemEntry)obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
    
    private bool Equals(FileSystemEntry other)
    {
        return Name == other.Name;
    }

}