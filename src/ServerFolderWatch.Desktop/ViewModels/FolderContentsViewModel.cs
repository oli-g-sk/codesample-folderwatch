using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Desktop.Messages;
using ServerFolderWatch.Desktop.Services;
using ServerFolderWatch.Desktop.ViewModels.Items;

namespace ServerFolderWatch.Desktop.ViewModels;

public partial class FolderContentsViewModel : ObservableObject,
    IRecipient<SelectedFolderChangedMsg>
{
    private readonly IFolderSnapshotService folderSnapshotService;
    private readonly IBrowseService browseService;

    public DispatcherCollection<BaseEntryViewModel> Entries { get; }

    [ObservableProperty]
    private bool isRefreshing;

    public FolderContentsViewModel(IFolderSnapshotService folderSnapshotService,
        IBrowseService browseService,
        IDispatcherService dispatcherService)
    {
        this.folderSnapshotService = folderSnapshotService;
        this.browseService = browseService;

        Entries = new DispatcherCollection<BaseEntryViewModel>(dispatcherService);
        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(SelectedFolderChangedMsg message)
    {
        _ = RefreshAsync(message);
    }

    private async Task RefreshAsync(SelectedFolderChangedMsg message)
    {
        IsRefreshing = true;
        await Entries.ClearAsync();
        
        // although still refreshing, we can enable interaction now
        IsRefreshing = false;

        if (message.Folder is { CanViewContents: true } folder)
        {
            var selectedFolderPath = Path.Combine(folder.BasePath, folder.Entry.Name);
            bool canRead = browseService.CanReadFolderContents(selectedFolderPath);
            
            if (!canRead)
                return;
            
            var contents = folderSnapshotService.GetCurrentContents(selectedFolderPath);
            await Entries.AddRange(EnumerateEntries(contents, selectedFolderPath));
        }
    }

    private IEnumerable<BaseEntryViewModel> EnumerateEntries(FolderSnapshot snapshot, string selectedFolderPath)
    {
        foreach (var entry in snapshot.Subfolders)
        {
            string subfolderPath = Path.Combine(selectedFolderPath, entry.Name);
            bool canReadSubfolder = browseService.CanReadFolderContents(subfolderPath);
            yield return new FolderViewModel(entry, selectedFolderPath, canReadSubfolder);
        }

        foreach (var entry in snapshot.VersionedFiles)
            yield return new FileViewModel(entry, selectedFolderPath);
    }
}
