using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Desktop.Messages;
using ServerFolderWatch.Desktop.ViewModels.Items;

namespace ServerFolderWatch.Desktop.ViewModels;

public partial class FolderTreeViewModel : ObservableObject
{
    private readonly IBrowseService browseService;

    private string? rootPath;
    
    public ObservableCollection<FolderViewModel> Folders { get; }
    
    [ObservableProperty]
    private FolderViewModel? selectedFolder;

    public FolderTreeViewModel(IBrowseService browseService)
    {
        this.browseService = browseService;
        Folders = new ObservableCollection<FolderViewModel>();
    }

    public async Task Initialize(string path)
    {
        if (rootPath != null)
        {
            // TODO support re-initializing when switching drives
            throw new InvalidOperationException("FolderTreeViewModel was already initialized.");
        }

        rootPath = path;
        var folders = browseService.GetSubfolders(path);

        foreach (var folder in folders)
        {
            // TODO use dispatcher collection
            Folders.Add(CreateFolderViewModel(folder, null));
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(SelectedFolder) && SelectedFolder is { } folder)
            WeakReferenceMessenger.Default.Send(new SelectedFolderChangedMsg(folder));
    }

    private void Folder_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is FolderViewModel folder && e.PropertyName == nameof(FolderViewModel.IsExpanded))
        {
            if (folder is { IsExpanded: true, HasChildren: true, ChildrenLoaded: false })
            {
                folder.Children.Clear();
    
                var children = browseService.GetSubfolders(folder.FullPath);

                foreach (var child in children) 
                    folder.Children.Add(CreateFolderViewModel(child, folder));
            }
        }
    }

    private FolderViewModel CreateFolderViewModel(Folder model, FolderViewModel? parent)
    {
        string parentPath = parent?.FullPath ?? rootPath
            ?? throw new InvalidOperationException("FolderTreeViewModel must be initialized before creating folders.");
        string fullPath = Path.Combine(parentPath, model.Name);
        bool canRead = browseService.CanReadFolderContents(fullPath);
        bool hasChildren = canRead && browseService.GetChildren(fullPath).Any();
        var viewModel = new FolderViewModel(model, fullPath, hasChildren, canRead);
        viewModel.PropertyChanged += Folder_OnPropertyChanged;
        return viewModel;
    }
}
