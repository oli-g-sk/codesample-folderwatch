using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Core.Service.Interfaces;

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
    /// <true/> if the folder was just initialized for monitoring,
    /// <false/> if it was already being monitored.
    /// </returns>
    Task<bool> Analyze(string monitoredPath);
    
    /// <summary>
    /// Existing entries (subfolders or files) currently in the folder last set
    /// by <see cref="Analyze(string)"/>.
    /// </summary>
    IList<FileSystemEntry> CurrentEntries { get; }
    
    /// <summary>
    /// All entries (subfolders or files) that are or were in this folder.
    /// This includes those that were since removed, but the last call to
    /// <see cref="Analyze(string)"/> still remembered a trace of them.
    /// This method is the source of truth for the purpose of highlighting changes.
    /// </summary>
    IList<FileSystemEntry> AllEntries { get; }
    
    ICollection<FileSystemEntry> AddedEntries { get; }
    
    ICollection<FileSystemEntry> ModifiedEntries { get; }
    
    ICollection<FileSystemEntry> DeletedEntries { get; }
}