using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Olivercode.WPFastr;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Desktop.Services;

namespace ServerFolderWatch.Desktop.ViewModels.Items;

public partial class FolderViewModel : BaseEntryViewModel
{
    public override bool IsFolder { get; } = true;
    
    public bool CanViewContents { get; }
    
    [ObservableProperty]
    private bool isExpanded;

    [ObservableProperty]
    private bool isSelected;
    
    [ObservableProperty]
    private bool hasChildren;

    /// <inheritdoc/>
    public FolderViewModel(FileSystemEntryBase entry, string fullPath, bool hasChildren, bool canViewContents,
        IDispatcherService dispatcherService) : base(entry, fullPath)
    {
        CanViewContents = canViewContents;
        this.hasChildren = hasChildren;
     
        Children = new DispatcherCollection<BaseEntryViewModel?>(dispatcherService);

        if (hasChildren)
        {
            // add a dummy item so that a hierarchical UI template expects children
            Children.Add(null);
        }
    }

    public DispatcherCollection<BaseEntryViewModel?> Children { get; }
    
    public bool ChildrenLoaded => HasChildren && Children is not [null];
}
