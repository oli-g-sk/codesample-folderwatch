using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Desktop.Messages;

namespace ServerFolderWatch.Desktop.ViewModels;

public class FolderContentsViewModel : ObservableObject,
    IRecipient<SelectedFolderChangedMsg>
{
    private readonly IFolderSnapshotService folderSnapshotService;

    public ObservableCollection<FileSystemEntryBase> Entries { get; } = [];

    public FolderContentsViewModel(IFolderSnapshotService folderSnapshotService)
    {
        this.folderSnapshotService = folderSnapshotService;
        
        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(SelectedFolderChangedMsg message)
    {
        Entries.Clear();

        if (message.Folder is { } folder)
        {
            var combinedPath = Path.Combine(folder.BasePath, folder.Entry.Name);
            var contents = folderSnapshotService.GetCurrentContents(combinedPath);
            
            foreach (var entry in contents.Subfolders)
                Entries.Add(entry);
            foreach (var entry in contents.VersionedFiles)
                Entries.Add(entry);
        }
    }
}