using System.Collections.Generic;
using System.Linq;
using Mapster;
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
    : PathControllerBase(browseService, loggerFactory)
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
            Changes = diff.Select(kvp => kvp.Adapt<FileSystemEntryDiffDto>())
                .OrderBy(x => x.Name)
                .ToList()
        });
    }
}