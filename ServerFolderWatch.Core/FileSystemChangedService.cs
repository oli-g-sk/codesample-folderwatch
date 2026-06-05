namespace ServerFolderWatch.Core;

public class FileSystemChangedService : IFileSystemChangeService
{
    public Task Setup(string monitoredPath)
    {
        throw new NotImplementedException();
    }

    public List<string> GetAddedEntries()
    {
        throw new NotImplementedException();
    }

    public List<string> GetModifiedEntries()
    {
        throw new NotImplementedException();
    }

    public List<string> GetDeletedEntries()
    {
        throw new NotImplementedException();
    }
}