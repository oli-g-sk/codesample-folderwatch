using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Desktop.ViewModels.Items;

public class FolderViewModel(FileSystemEntryBase entry, string basePath) : BaseEntryViewModel(entry, basePath)
{
    public override bool IsFolder { get; } = true;
}