using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Server.DTOs;

namespace ServerFolderWatch.Server.Controllers;

public class DiffController(IBrowseService browseService,
    IFolderSnapshotService folderSnapshotService,
    IFolderDiffService folderDiffService,
    ILoggerFactory loggerFactory)
    : BaseController(browseService, loggerFactory)
{
            
    [HttpGet("api/diff")]
    public IActionResult Diff([FromQuery(Name = "folder")] string? path)
    {
        path = CoercePath(path);
        
        if (!ValidateRequest(path, out var error))
            return error!;

        var previousSnapshot =
            folderSnapshotService.IsFolderAlreadyMonitored(path)
                ? folderSnapshotService.LoadPersistedSnapshot(path)
                : null;

        var currentContents = folderSnapshotService.GetCurrentContents(path);
        var diff = previousSnapshot == null
            ? new Dictionary<FileSystemEntryBase, DiffOperation>()
            : folderDiffService.Compare(previousSnapshot, currentContents, path, out _);
    
        return Ok(new DiffResponseDto()
        {
            Path = path,
            LastAnalyzed = previousSnapshot?.LastAnalyzed,
            Changes = MapDiffedEntries(diff)
        });
    }    
    
    private static IEnumerable<FileSystemEntryDiffDto> MapDiffedEntries(IDictionary<FileSystemEntryBase, DiffOperation> diff)
    {
        var ordered = diff.OrderBy(x => x.Key);
        
        return ordered.Select(entry => new FileSystemEntryDiffDto(
            entry.Key.Name,
            entry.Key is File
                ? FileSystemEntityType.File
                : FileSystemEntityType.Directory,
            entry.Value,
            entry.Key is File file
                ? file.Version
                : null
        ));
    }
}