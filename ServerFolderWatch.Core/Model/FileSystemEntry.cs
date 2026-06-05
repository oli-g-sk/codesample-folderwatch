namespace ServerFolderWatch.Core.Model;

public abstract class FileSystemEntry(string name) : IComparable<FileSystemEntry>
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

    public int CompareTo(FileSystemEntry? other)
    {
        if (ReferenceEquals(this, other)) return 0;

        return other switch
        {
            File => -1,
            null or Directory => 1,
            _ => string.Compare(Name, other.Name, StringComparison.Ordinal)
        };
    }
}