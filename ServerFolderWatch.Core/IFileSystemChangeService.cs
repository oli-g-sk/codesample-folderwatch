namespace ServerFolderWatch.Core;

public interface IFileSystemChangeService
{
    /// <summary>
    /// Set up folder monitoring in the provided <see cref="monitoredPath"/>,
    /// if not already set up (either directly here, or in a parent folder).
    /// </summary>
    /// <param name="monitoredPath"></param>
    /// <returns>
    /// True if the folder was just set up for monitoring,
    /// false if this or a parent folder was already set up in the past.
    /// </returns>
    Task<bool> Setup(string monitoredPath);
    
    List<string> GetAddedEntries();
    
    List<string> GetModifiedEntries();
    
    List<string> GetDeletedEntries();
}