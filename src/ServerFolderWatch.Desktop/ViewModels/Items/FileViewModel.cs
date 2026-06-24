using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Desktop.ViewModels.Items;

public class FileViewModel(FileSystemEntryBase entry, string fullPath) : BaseEntryViewModel(entry, fullPath)
{
    public override bool IsFolder { get; } = false;

    public override string Icon => "📄";
}
