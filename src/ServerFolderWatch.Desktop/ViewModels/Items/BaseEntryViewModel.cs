using CommunityToolkit.Mvvm.ComponentModel;
using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Desktop.ViewModels.Items;

public abstract class BaseEntryViewModel(FileSystemEntryBase entry, string basePath)
    : ObservableObject
{
    public FileSystemEntryBase Entry { get; } = entry;

    // TODO replace with a Parent reference
    public string BasePath { get; } = basePath;

    public abstract bool IsFolder { get; }
}