using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Desktop.Messages;

namespace ServerFolderWatch.Desktop.ViewModels;

public partial class FolderTreeViewModel : ObservableObject
{
    private readonly IBrowseService browseService;
    
    public ObservableCollection<Folder> Folders { get; }
    
    [ObservableProperty]
    private Folder? selectedFolder;

    public FolderTreeViewModel(IBrowseService browseService)
    {
        this.browseService = browseService;
        Folders = new ObservableCollection<Folder>();
    }

    public async Task Initialize(string path)
    {
        Folders.Clear();
        var folders = browseService.GetSubfolders(path);

        foreach (var folder in folders)
        {
            await Task.Delay(250);
            Folders.Add(folder);           
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(SelectedFolder))
            WeakReferenceMessenger.Default.Send(new SelectedFolderChangedMsg(SelectedFolder));
    }
}