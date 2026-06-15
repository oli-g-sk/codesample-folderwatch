using ServerFolderWatch.Core.Model;

namespace ServerFolderWatch.Desktop.Messages;

public record SelectedFolderChangedMsg(Folder? Folder);
