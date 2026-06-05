using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service;

public interface IFileSystemDiffService
{
    /// <summary>
    /// Analyze (or set up) this folder for file system changes tracking.
    /// If the folder or its parents were already taking part in the change tracking,
    /// changes since the last call will be set into <see cref="AddedEntries"/>,
    /// <see cref="ModifiedEntries"/>, and <see cref="DeletedEntries"/>" respectively.
    /// </summary>
    /// <param name="monitoredPath"></param>
    /// <returns>
    /// Whether file system changes were already being tracked for this folder.
    /// </returns>
    Task<bool> Analyze(string monitoredPath);
    
    ICollection<FileSystemEntry> AddedEntries { get; }
    
    ICollection<FileSystemEntry> ModifiedEntries { get; }
    
    ICollection<FileSystemEntry> DeletedEntries { get; }
}