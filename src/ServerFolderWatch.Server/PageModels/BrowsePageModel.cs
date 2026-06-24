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

    public IEnumerable<BreadcrumbViewModel> GetBreadcrumbs()
    {
        if (string.IsNullOrWhiteSpace(currentFolder))
            yield break;

        var path = string.Empty;

        foreach (string segment in currentFolder.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            path = string.IsNullOrWhiteSpace(path)
                ? segment
                : $"{path}/{segment}";

            yield return new BreadcrumbViewModel(
                segment,
                GetBrowseUrl(path),
                "📂",
                folderSnapshotService.IsFolderAlreadyMonitored(path));
        }
    }

    public IDictionary<FileSystemEntryBase, DiffOperation> GetDiffEntries()
    {
        var contents = folderSnapshotService.GetCurrentContents(currentFolder ?? string.Empty);
        var oldSnapshot = folderSnapshotService.LoadPersistedSnapshot(currentFolder);
        
        // TODO show stats in status bar
        return folderDiffService.Compare(oldSnapshot, contents, currentFolder, out var stats);
    }

    public string GetFolderUrl(Folder childFolder)
    {
        return GetBrowseUrl(GetFolderPath(childFolder));
    }

    public string GetFolderIcon(Folder childFolder)
    {
        return GetFolderPath(childFolder).Equals(currentFolder, StringComparison.OrdinalIgnoreCase)
            ? "📂"
            : "📁";
    }

    public bool IsFolderMonitored(Folder childFolder)
    {
        return folderSnapshotService.IsFolderAlreadyMonitored(GetFolderPath(childFolder));
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

    private string GetFolderPath(Folder childFolder)
    {
        return string.IsNullOrWhiteSpace(currentFolder)
            ? childFolder.Name
            : $"{currentFolder}/{childFolder.Name}";
    }
}
