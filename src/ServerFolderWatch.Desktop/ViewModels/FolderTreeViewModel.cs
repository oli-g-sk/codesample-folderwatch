using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Desktop.Messages;
using ServerFolderWatch.Desktop.ViewModels.Items;

namespace ServerFolderWatch.Desktop.ViewModels;

public partial class FolderTreeViewModel : ObservableObject
{
    private readonly IBrowseService browseService;

    private string rootPath;
    
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
        Folders.Clear();
        var folders = browseService.GetSubfolders(path);

        foreach (var folder in folders)
        {
            // TODO use dispatcher collection
            string folderPath = Path.Combine(path, folder.Name);
            bool canRead = browseService.CanReadFolderContents(folderPath);
            bool hasChildren = canRead && browseService.GetChildren(folderPath).Any();
            Folders.Add(new FolderViewModel(folder, path, hasChildren, canRead));
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(SelectedFolder) && SelectedFolder is { } folder)
            WeakReferenceMessenger.Default.Send(new SelectedFolderChangedMsg(folder));
    }
}