using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private readonly IDispatcherService dispatcherService;

    public DispatcherCollection<BaseEntryViewModel> Entries { get; }

    [ObservableProperty]
    private bool isRefreshing;

    public FolderContentsViewModel(IFolderSnapshotService folderSnapshotService,
        IBrowseService browseService,
        IDispatcherService dispatcherService)
    {
        this.folderSnapshotService = folderSnapshotService;
        this.browseService = browseService;
        this.dispatcherService = dispatcherService;

        Entries = new DispatcherCollection<BaseEntryViewModel>(dispatcherService);
        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(SelectedFolderChangedMsg message)
    {
        _ = RefreshAsync(message);
    }

    [RelayCommand]
    private void OpenEntry(BaseEntryViewModel? entry)
    {
        if (entry is FolderViewModel { CanViewContents: true } folder)
            WeakReferenceMessenger.Default.Send(new SelectedFolderChangedMsg(folder));
    }

    private async Task RefreshAsync(SelectedFolderChangedMsg message)
    {
        var updateVersion = Entries.BeginUpdate();
        IsRefreshing = true;

        try
        {
            await Entries.ClearAsync(updateVersion);

            if (!Entries.IsCurrent(updateVersion))
                return;

            if (message.Folder is { CanViewContents: true } folder)
            {
                bool canRead = browseService.CanReadFolderContents(folder.FullPath);

                if (!canRead || !Entries.IsCurrent(updateVersion))
                    return;

                var contents = folderSnapshotService.GetCurrentContents(folder.FullPath);
                await Entries.AddRangeAsync(EnumerateEntries(contents, folder.FullPath), updateVersion);
            }
        }
        finally
        {
            if (Entries.IsCurrent(updateVersion))
                IsRefreshing = false;
        }
    }

    private IEnumerable<BaseEntryViewModel> EnumerateEntries(FolderSnapshot snapshot, string selectedFolderPath)
    {
        foreach (var entry in snapshot.Subfolders)
        {
            string fullPath = Path.Combine(selectedFolderPath, entry.Name);
            bool canReadSubfolder = browseService.CanReadFolderContents(fullPath);
            bool hasChildren = canReadSubfolder && browseService.GetChildren(fullPath).Any();
            yield return new FolderViewModel(entry, fullPath, hasChildren, canReadSubfolder, dispatcherService);
        }

        foreach (var entry in snapshot.VersionedFiles)
            yield return new FileViewModel(entry, Path.Combine(selectedFolderPath, entry.Name));
    }
}
