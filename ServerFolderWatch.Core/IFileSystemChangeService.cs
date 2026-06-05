namespace ServerFolderWatch.Core;

public interface IFileSystemChangeService
{
    Task<bool> Setup(string monitoredPath);
    
    List<string> GetAddedEntries();
    
    List<string> GetModifiedEntries();
    
    List<string> GetDeletedEntries();
}