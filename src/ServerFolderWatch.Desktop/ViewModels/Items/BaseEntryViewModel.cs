using CommunityToolkit.Mvvm.ComponentModel;
using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Desktop.ViewModels.Items;

public abstract class BaseEntryViewModel(FileSystemEntryBase entry, string fullPath)
    : ObservableObject
{
    public FileSystemEntryBase Entry { get; } = entry;
    
    public string FullPath { get; } = fullPath;
    
    public string Name => Entry.Name;

    public abstract bool IsFolder { get; }
}
