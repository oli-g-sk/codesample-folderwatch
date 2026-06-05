namespace ServerFolderWatch.Core;

public interface IFileSystemChangeService
{
    bool IsSetup(string monitoredPath);
    
    Task Setup(string monitoredPath);

    List<string> GetAddedEntries();
    
    List<string> GetModifiedEntries();
    
    List<string> GetDeletedEntries();
}