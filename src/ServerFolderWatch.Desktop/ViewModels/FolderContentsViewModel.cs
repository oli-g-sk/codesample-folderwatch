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
    private readonly IFolderDiffService folderDiffService;
    private readonly IBrowseService browseService;
    private readonly IDispatcherService dispatcherService;

    public DispatcherCollection<BaseEntryViewModel> Entries { get; }

    [ObservableProperty]
    private bool isBusy;

    public FolderContentsViewModel(IFolderSnapshotService folderSnapshotService,
        IFolderDiffService folderDiffService,
        IBrowseService browseService,
        IDispatcherService dispatcherService)
    {
        this.folderSnapshotService = folderSnapshotService;
        this.folderDiffService = folderDiffService;
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
            var previousContents = folderSnapshotService.LoadPersistedSnapshot(folder.FullPath);
            var diff = folderDiffService.Compare(previousContents, contents, folder.FullPath, out _);
            await Entries.ReplaceRangeAsync(EnumerateEntries(diff, folder.FullPath), updateVersion);
        }
        else
        {
            await Entries.ClearAsync(updateVersion);
        }
    }

    private IEnumerable<BaseEntryViewModel> EnumerateEntries(
        IReadOnlyDictionary<FileSystemEntryBase, DiffOperation> diff, string selectedFolderPath)
    {
        foreach (var (entry, operation) in diff)
        {
            string fullPath = Path.Combine(selectedFolderPath, entry.Name);

            if (entry is Folder)
            {
                bool canRead = operation != DiffOperation.Removed && browseService.CanReadFolderContents(fullPath);
                bool hasChildren = canRead && browseService.GetChildren(fullPath).Any();
                yield return new FolderViewModel(entry, fullPath, hasChildren, canRead, dispatcherService)
                    { DiffOperation = operation };
            }
            else
            {
                yield return new FileViewModel(entry, fullPath) { DiffOperation = operation };
            }
        }
    }
}
