namespace ServerFolderWatch.Desktop.ViewModels;

public class BrowseViewModel(
    FolderTreeViewModel folderTree,
    FolderContentsViewModel folderContents)
{
    public FolderTreeViewModel FolderTree { get; } = folderTree;

    public FolderContentsViewModel FolderContents { get; } = folderContents;
}