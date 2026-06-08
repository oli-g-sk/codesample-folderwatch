namespace ServerFolderWatch.Core.Service.Interfaces;

public interface IBrowseService
{
    bool CanReadFolderContents(string path);
    
    bool CanWriteToFolder(string path);

    bool CanGoToParent(string path);
}