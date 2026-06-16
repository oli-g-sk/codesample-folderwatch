using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Desktop.Messages;
using ServerFolderWatch.Desktop.ViewModels.Items;

namespace ServerFolderWatch.Desktop.ViewModels;

public partial class FolderTreeViewModel : ObservableObject,
    IRecipient<SelectedFolderChangedMsg>
{
    private readonly IBrowseService browseService;

    private string? rootPath;
    private bool suppressSelectedFolderMessage;
    
    public ObservableCollection<FolderViewModel> Folders { get; }
    
    [ObservableProperty]
    private FolderViewModel? selectedFolder;

    public FolderTreeViewModel(IBrowseService browseService)
    {
        this.browseService = browseService;
        Folders = new ObservableCollection<FolderViewModel>();
        WeakReferenceMessenger.Default.Register(this);
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

        if (!suppressSelectedFolderMessage
            && e.PropertyName == nameof(SelectedFolder)
            && SelectedFolder is { } folder)
        {
            WeakReferenceMessenger.Default.Send(new SelectedFolderChangedMsg(folder));
        }
    }

    public void Receive(SelectedFolderChangedMsg message)
    {
        if (message.Folder is not { } folder
            || SelectedFolder?.FullPath == folder.FullPath)
        {
            return;
        }

        var treeFolder = FindOrLoadFolder(folder.FullPath);

        if (treeFolder is not null)
            SelectFolder(treeFolder, publishMessage: false);
    }

    private void Folder_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is FolderViewModel folder && e.PropertyName == nameof(FolderViewModel.IsExpanded))
        {
            if (folder is { IsExpanded: true, HasChildren: true, ChildrenLoaded: false })
                LoadChildren(folder);
        }
    }

    private FolderViewModel? FindOrLoadFolder(string fullPath)
    {
        foreach (var folder in Folders)
        {
            var match = FindOrLoadFolder(folder, fullPath);

            if (match is not null)
                return match;
        }

        return null;
    }

    private FolderViewModel? FindOrLoadFolder(FolderViewModel folder, string fullPath)
    {
        if (folder.FullPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase))
            return folder;

        if (!IsUnderFolder(fullPath, folder.FullPath))
            return null;

        if (folder.HasChildren)
        {
            LoadChildren(folder);
            folder.IsExpanded = true;
        }

        foreach (var child in folder.Children.OfType<FolderViewModel>())
        {
            var match = FindOrLoadFolder(child, fullPath);

            if (match is not null)
                return match;
        }

        return null;
    }

    private void LoadChildren(FolderViewModel folder)
    {
        if (!folder.HasChildren || folder.ChildrenLoaded)
            return;

        folder.Children.Clear();

        var children = browseService.GetSubfolders(folder.FullPath);

        foreach (var child in children)
            folder.Children.Add(CreateFolderViewModel(child, folder));
    }

    private void SelectFolder(FolderViewModel folder, bool publishMessage)
    {
        if (SelectedFolder is { } selectedFolder)
            selectedFolder.IsSelected = false;

        try
        {
            suppressSelectedFolderMessage = !publishMessage;
            SelectedFolder = folder;
            folder.IsSelected = true;
        }
        finally
        {
            suppressSelectedFolderMessage = false;
        }
    }

    private static bool IsUnderFolder(string fullPath, string folderPath)
    {
        var normalizedFolderPath = Path.TrimEndingDirectorySeparator(folderPath);

        return fullPath.StartsWith(normalizedFolderPath + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase);
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
