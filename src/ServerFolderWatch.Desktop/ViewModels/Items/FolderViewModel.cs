using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Desktop.ViewModels.Items;

public class FolderViewModel(FileSystemEntryBase entry, string basePath, bool canViewContents) : BaseEntryViewModel(entry, basePath)
{
    public override bool IsFolder { get; } = true;
    
    public bool CanViewContents { get; } = canViewContents;
}