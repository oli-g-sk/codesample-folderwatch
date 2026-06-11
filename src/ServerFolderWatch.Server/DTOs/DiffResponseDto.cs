using System;
using System.Collections.Generic;

namespace ServerFolderWatch.Server.DTOs;

public class DiffResponseDto : FolderContentResponseBase
{
    public DateTime? LastAnalyzed { get; set; }
    
    public IEnumerable<FileSystemEntryDiffDto>? Changes { get; set; }
}