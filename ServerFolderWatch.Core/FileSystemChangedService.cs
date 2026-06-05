using System.IO.Abstractions;

namespace ServerFolderWatch.Core;

public class FileSystemChangedService(IPath path, IDirectory directory, IFile file, IConfiguration configuration)
    : IFileSystemChangeService
{
    private readonly IPath path = path;
    private readonly IDirectory directory = directory;
    private readonly IFile file = file;
    private readonly IConfiguration configuration = configuration;

    public Task<bool> Setup(string monitoredPath)
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