using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Desktop.Messages;
using ServerFolderWatch.Desktop.Services;
using ServerFolderWatch.Desktop.ViewModels.Items;

namespace ServerFolderWatch.Desktop.ViewModels;

public class FolderContentsViewModel : ObservableObject,
    IRecipient<SelectedFolderChangedMsg>
{
    private readonly IFolderSnapshotService folderSnapshotService;
    private readonly IBrowseService browseService;
    private readonly IDispatcherService dispatcherService;

    public ObservableCollection<BaseEntryViewModel> Entries { get; } = [];

    public FolderContentsViewModel(IFolderSnapshotService folderSnapshotService,
        IBrowseService browseService,
        IDispatcherService dispatcherService)
    {
        this.folderSnapshotService = folderSnapshotService;
        this.browseService = browseService;
        this.dispatcherService = dispatcherService;

        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(SelectedFolderChangedMsg message)
    {
        Entries.Clear();

        if (message.Folder is { } folder)
        {
            var selectedFolderPath = Path.Combine(folder.BasePath, folder.Entry.Name);
            bool canRead = browseService.CanReadFolderContents(selectedFolderPath);
            
            if (!canRead)
                return;
            
            var contents = folderSnapshotService.GetCurrentContents(selectedFolderPath);

            foreach (var entry in EnumerateEntries(contents, selectedFolderPath))
                dispatcherService.InvokeAsync(() => Entries.Add(entry), IDispatcherService.BackgroundPriority);
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
