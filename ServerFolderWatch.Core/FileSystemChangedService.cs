using System.IO.Abstractions;

namespace ServerFolderWatch.Core;

public class FileSystemChangedService(IPath path, IDirectory directory, IFile file)
    : IFileSystemChangeService
{
    private readonly IPath path = path;
    private readonly IDirectory directory = directory;
    private readonly IFile file = file;

    public bool IsSetup(string monitoredPath)
    {
        throw new NotImplementedException();
    }

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