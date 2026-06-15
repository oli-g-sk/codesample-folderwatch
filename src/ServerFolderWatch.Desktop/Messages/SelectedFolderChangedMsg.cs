using ServerFolderWatch.Desktop.ViewModels.Items;

namespace ServerFolderWatch.Desktop.Messages;

public record SelectedFolderChangedMsg(FolderViewModel? Folder);
