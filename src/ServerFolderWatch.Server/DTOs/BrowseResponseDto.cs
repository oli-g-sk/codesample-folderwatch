using System.Collections.Generic;

namespace ServerFolderWatch.Server.DTOs;

public class BrowseResponseDto : FolderContentResponseBase
{
    public IEnumerable<FileSystemEntryDto>? Contents { get; init; }
}