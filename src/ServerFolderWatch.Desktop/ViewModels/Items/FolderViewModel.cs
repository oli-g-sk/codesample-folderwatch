using CommunityToolkit.Mvvm.ComponentModel;
using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Desktop.ViewModels.Items;

public partial class FolderViewModel(FileSystemEntryBase entry, string basePath, bool canViewContents)
    : BaseEntryViewModel(entry, basePath)
{
    public override bool IsFolder { get; } = true;
    
    public bool CanViewContents { get; } = canViewContents;
    
    [ObservableProperty]
    private bool isExpanded;
}