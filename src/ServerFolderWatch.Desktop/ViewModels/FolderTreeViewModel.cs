using System.Collections.ObjectModel;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;

namespace ServerFolderWatch.Desktop.ViewModels;

public class FolderTreeViewModel
{
    private readonly IBrowseService browseService;
    
    public ObservableCollection<Folder> Folders { get; }

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
}