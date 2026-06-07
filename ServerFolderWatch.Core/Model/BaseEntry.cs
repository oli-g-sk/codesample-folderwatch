namespace ServerFolderWatch.Core.Model;

public abstract class BaseEntry(string name) : IComparable<BaseEntry>
{
    public string Name { get; } = name;

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((BaseEntry)obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
    
    private bool Equals(BaseEntry other)
    {
        return Name == other.Name;
    }

    public int CompareTo(BaseEntry? other)
    {
        if (ReferenceEquals(this, other))
            return 0;

        if (other is null)
            return 1;

        if (this is Folder && other is File)
            return -1;

        if (this is File && other is Folder)
            return 1;

        return string.Compare(Name, other.Name, StringComparison.Ordinal);
    }
}