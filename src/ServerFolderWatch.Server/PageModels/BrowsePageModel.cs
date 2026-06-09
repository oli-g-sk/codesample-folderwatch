using System;
using System.Collections.Generic;
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
        currentFolder = folderParam;
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
        if (string.IsNullOrWhiteSpace(currentFolder))
            return string.Empty;

        var lastSeparator = currentFolder.LastIndexOf('/');
        return lastSeparator < 0
            ? string.Empty
            : currentFolder[..lastSeparator];
    }

    public string GetParentUrl()
    {
        return GetBrowseUrl(GetParentPath());
    }

    public List<(BaseEntry FileSystemEntry, DiffOperation Operation)> GetDiffEntries()
    {
        var contents = folderSnapshotService.GetCurrentContents(currentFolder ?? string.Empty);
        var oldSnapshot = folderSnapshotService.LoadPersistedSnapshot(currentFolder);
        
        // TODO show stats in status bar
        return folderDiffService.Compare(oldSnapshot, contents, currentFolder, out var stats)
            .Entries;
    }

    public string GetFolderUrl(Folder childFolder)
    {
        var childPath = string.IsNullOrWhiteSpace(currentFolder)
            ? childFolder.Name
            : $"{currentFolder}/{childFolder.Name}";

        return GetBrowseUrl(childPath);
    }

    public bool CanGoToParent()
    {
        return browseService.CanGoToParent(currentFolder ?? string.Empty);
    }

    private static string GetBrowseUrl(string? folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            return "/browse";

        return $"/browse?folder={Uri.EscapeDataString(folderPath)}";
    }
}
