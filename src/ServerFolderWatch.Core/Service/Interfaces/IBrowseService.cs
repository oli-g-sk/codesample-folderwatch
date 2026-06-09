using ServerFolderWatch.Core.Model;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Core.Service.Interfaces;

public interface IBrowseService
{
    string GetFileSystemPath(string? folderPath);
    
    bool FolderExists(string folderPath);
    
    /// <summary>
    /// Returns fully qualified sub-entries of the given <see cref="folderPath"/>
    /// </summary>
    IEnumerable<string> GetChildren(string folderPath);

    /// <summary>
    /// Returns alphabetically sorted sub-folders of the given
    /// <see cref="folderPath"/> as model <see cref="Folder"/> objects. 
    /// </summary>
    IList<Folder> GetSubfolders(string folderPath);
    
    /// <summary>
    /// Returns alphabetically sorted files within the given
    /// <see cref="folderPath"/> as model <see cref="File"/> objects. 
    /// </summary>
    IList<File> GetFiles(string folderPath);
    
    // TODO add tests for all actually paths being folders
    
    bool CanReadFolderContents(string folderPath);
    
    bool CanWriteToFolder(string folderPath);

    bool CanGoToParent(string folderPath);
}
