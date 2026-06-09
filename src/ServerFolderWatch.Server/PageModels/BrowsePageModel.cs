using System.Collections.Generic;
using System.Web;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;

namespace ServerFolderWatch.Server.PageModels;

public class BrowsePageModel(
    IFolderSnapshotService folderSnapshotService,
    IFolderDiffService folderDiffService,
    IBrowseService browseService)
{
    private string? currentFolder;
    
    public void LoadFolder(string? folderParam)
    {
        currentFolder = folderParam ?? string.Empty;
    }
    
    public string GetClassName(DiffOperation diffOperation)
    {
        return diffOperation switch
        {
            DiffOperation.Added => "diff-added",
            DiffOperation.Removed => "diff-removed",
            DiffOperation.Modified => "diff-modified",
            _ => ""
        };
    }

    public string GetParentPath()
    {
        if (string.IsNullOrWhiteSpace(currentFolder) || !currentFolder.Contains('/'))
            return string.Empty;
        
        return currentFolder?[..currentFolder.IndexOf('/')] ?? "/";
    }

    public string GetParentUrl()
    {
        var parentPath = GetParentPath();
        return string.IsNullOrEmpty(parentPath) ? "/browse" : $"/browse?folder={parentPath}";
    }

    public List<(BaseEntry FileSystemEntry, DiffOperation Operation)> GetDiffEntries()
    {
        var contents = folderSnapshotService.GetCurrentContents(currentFolder ?? string.Empty);
        var oldSnapshot = folderSnapshotService.LoadPersistedSnapshot(currentFolder);
        
        // TODO show stats in status bar
        return folderDiffService.Compare(oldSnapshot, contents, currentFolder, out var stats)
            .Entries;
    }

    public string GetChildPath(Folder childFolder)
    {
        return $"{currentFolder}/{HttpUtility.UrlEncode(childFolder.Name)}";
    }

    public bool CanGoToParent()
    {
        return browseService.CanGoToParent(currentFolder ?? string.Empty);
    }
}