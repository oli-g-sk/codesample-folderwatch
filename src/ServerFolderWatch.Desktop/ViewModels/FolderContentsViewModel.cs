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
    private readonly IFileLauncher fileLauncher;

    public DispatcherCollection<BaseEntryViewModel> Entries { get; }

    [ObservableProperty]
    private IEnumerable<FolderViewModel> breadcrumbs = [];

    [ObservableProperty]
    private bool isBusy;

    public FolderContentsViewModel(IFolderSnapshotService folderSnapshotService,
        IFolderDiffService folderDiffService,
        IBrowseService browseService,
        IDispatcherService dispatcherService,
        IFileLauncher fileLauncher)
    {
        this.folderSnapshotService = folderSnapshotService;
        this.folderDiffService = folderDiffService;
        this.browseService = browseService;
        this.dispatcherService = dispatcherService;
        this.fileLauncher = fileLauncher;

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
        else if (entry is FileViewModel)
            fileLauncher.Open(entry.FullPath);
    }

    private async Task RefreshAsync(SelectedFolderChangedMsg message)
    {
        var updateVersion = Entries.BeginUpdate();

        if (message.Folder is { CanViewContents: true } folder)
        {
            Breadcrumbs = EnumerateBreadcrumbs(folder);
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
            await Entries.ReplaceRangeAsync(EnumerateEntries(diff, folder), updateVersion);
        }
        else
        {
            Breadcrumbs = [];
            await Entries.ClearAsync(updateVersion);
        }
    }

    private IEnumerable<BaseEntryViewModel> EnumerateEntries(
        IReadOnlyDictionary<FileSystemEntryBase, DiffOperation> diff, FolderViewModel selectedFolder)
    {
        foreach (var (entry, operation) in diff)
        {
            string fullPath = Path.Combine(selectedFolder.FullPath, entry.Name);

            if (entry is Folder)
            {
                bool canRead = operation != DiffOperation.Removed && browseService.CanReadFolderContents(fullPath);
                bool hasChildren = canRead && browseService.GetChildren(fullPath).Any();
                bool isMonitored = folderSnapshotService.IsFolderAlreadyMonitored(fullPath);
                yield return new FolderViewModel(entry, fullPath, hasChildren, canRead, isMonitored,
                    dispatcherService, selectedFolder)
                    { DiffOperation = operation };
            }
            else
            {
                yield return new FileViewModel(entry, fullPath) { DiffOperation = operation };
            }
        }
    }

    private static IEnumerable<FolderViewModel> EnumerateBreadcrumbs(FolderViewModel folder)
    {
        var result = new Stack<FolderViewModel>();

        for (FolderViewModel? current = folder; current is not null; current = current.Parent)
            result.Push(current);

        return result;
    }
}
