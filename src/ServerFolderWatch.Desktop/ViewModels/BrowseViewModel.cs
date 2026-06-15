namespace ServerFolderWatch.Desktop.ViewModels;

public class BrowseViewModel
{
    public FolderTreeViewModel FolderTree { get; }

    public BrowseViewModel(FolderTreeViewModel folderTree)
    {
        FolderTree = folderTree;
    }
}