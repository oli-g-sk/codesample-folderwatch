using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Desktop.Messages;
using ServerFolderWatch.Desktop.ViewModels.Items;

namespace ServerFolderWatch.Desktop.ViewModels;

public class FolderContentsViewModel : ObservableObject,
    IRecipient<SelectedFolderChangedMsg>
{
    private readonly IFolderSnapshotService folderSnapshotService;
    private readonly IBrowseService browseService;

    public ObservableCollection<BaseEntryViewModel> Entries { get; } = [];

    public FolderContentsViewModel(IFolderSnapshotService folderSnapshotService,
        IBrowseService browseService)
    {
        this.folderSnapshotService = folderSnapshotService;
        this.browseService = browseService;

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

            foreach (var entry in contents.Subfolders)
            {
                string subfolderPath = Path.Combine(selectedFolderPath, entry.Name);
                bool canReadSubfolder = browseService.CanReadFolderContents(subfolderPath);
                Entries.Add(new FolderViewModel(entry, selectedFolderPath, canReadSubfolder));
            }

            foreach (var entry in contents.VersionedFiles)
                Entries.Add(new FileViewModel(entry, selectedFolderPath));
        }
    }
}
