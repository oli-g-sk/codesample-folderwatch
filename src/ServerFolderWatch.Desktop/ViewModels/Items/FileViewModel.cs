using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Desktop.ViewModels.Items;

public class FileViewModel(FileSystemEntryBase entry, string basePath) : BaseEntryViewModel(entry, basePath)
{
    public override bool IsFolder { get; } = false;
}