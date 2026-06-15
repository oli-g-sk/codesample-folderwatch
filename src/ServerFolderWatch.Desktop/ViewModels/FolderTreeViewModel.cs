using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;

namespace ServerFolderWatch.Desktop.ViewModels;

public class FolderTreeViewModel
{
    private readonly IBrowseService browseService;

    public FolderTreeViewModel(IBrowseService browseService)
    {
        this.browseService = browseService;
    }

    public IList<Folder> GetSubFolders(string path)
    {
        return browseService.GetSubfolders(path);
    }
}