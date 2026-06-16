using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Desktop.ViewModels.Items;

public partial class FolderViewModel : BaseEntryViewModel
{
    public override bool IsFolder { get; } = true;
    
    public bool CanViewContents { get; }
    
    [ObservableProperty]
    private bool isExpanded;
    
    [ObservableProperty]
    private bool hasChildren;

    /// <inheritdoc/>
    public FolderViewModel(FileSystemEntryBase entry,
        string basePath, bool hasChildren, bool canViewContents) : base(entry, basePath)
    {
        CanViewContents = canViewContents;
        this.hasChildren = hasChildren;
        
        if (hasChildren)
            Children.Add(null);
    }

    public ObservableCollection<BaseEntryViewModel?> Children { get; } = [];
    
    public bool ChildrenLoaded => HasChildren && Children is not [null];
}