using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Olivercode.WPFastr;
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
    private bool isBusy;

    public FolderContentsViewModel(IFolderSnapshotService folderSnapshotService,
        IBrowseService browseService,
        IDispatcherService dispatcherService)
    {
        this.folderSnapshotService = folderSnapshotService;
        this.browseService = browseService;
        this.dispatcherService = dispatcherService;

        Entries = new DispatcherCollection<BaseEntryViewModel>(dispatcherService);
        Entries.IsBusyChanged += Entries_OnIsBusyChanged;
        IsBusy = Entries.IsBusy;
        WeakReferenceMessenger.Default.Register(this);
    }

    private void Entries_OnIsBusyChanged(object? sender, EventArgs e)
    {
        IsBusy = Entries.IsBusy;
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

        if (message.Folder is { CanViewContents: true } folder)
        {
            bool canRead = browseService.CanReadFolderContents(folder.FullPath);

            if (!canRead || !Entries.IsCurrent(updateVersion))
            {
                if (Entries.IsCurrent(updateVersion))
                    await Entries.ClearAsync(updateVersion);

                return;
            }

            var contents = folderSnapshotService.GetCurrentContents(folder.FullPath);
            await Entries.ReplaceRangeAsync(EnumerateEntries(contents, folder.FullPath), updateVersion);
        }
        else
        {
            await Entries.ClearAsync(updateVersion);
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
