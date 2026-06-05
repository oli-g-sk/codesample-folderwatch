namespace ServerFolderWatch.Core;

public interface IFileSystemChangeService
{
    Task Setup(string monitoredPath);

    List<string> GetAddedEntries();
    
    List<string> GetModifiedEntries();
    
    List<string> GetDeletedEntries();
}