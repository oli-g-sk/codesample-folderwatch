using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Server.DTOs;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Server.Controllers.ApiControllers;

[ApiController]
public class BrowseController(IBrowseService browseService,
    IFolderSnapshotService folderSnapshotService,
    IFolderDiffService folderDiffService,
    IConfiguration configuration,
    ILoggerFactory loggerFactory) : ControllerBase
{
    private readonly ILogger<BrowseController> logger = loggerFactory.CreateLogger<BrowseController>();
    
    [HttpGet("api/browse")]
    public IActionResult Browse([FromQuery(Name = "folder")] string? path)
    {
        if (!ValidateRequest(path, out var fullPath, out var error))
            return BadRequest(error);

        var currentContent = folderSnapshotService.GetCurrentContents(fullPath);
        
        return Ok(new
        {
            LastAnalyzed = currentContent.LastAnalyzed,
            Entries = MapCurrentEntries(currentContent.GetAllEntries())
        });
    }
        
    [HttpGet("api/diff")]
    public IActionResult Diff([FromQuery(Name = "folder")] string? path)
    {
        if (!ValidateRequest(path, out var fullPath, out var error))
            return BadRequest(error);

        var previousSnapshot =
            folderSnapshotService.IsFolderAlreadyMonitored(fullPath) ?
                folderSnapshotService.LoadPersistedSnapshot(fullPath)
                : FolderSnapshot.Empty;

        var currentContents = folderSnapshotService.GetCurrentContents(fullPath);
        var diff = folderDiffService.Compare(previousSnapshot, currentContents, fullPath, out _);

        return Ok(new
        {
            LastAnalyzed = currentContents.LastAnalyzed,
            Entries = MapDiffedEntries(diff)
        });
    }

    private static IEnumerable<FileSystemEntryDto> MapCurrentEntries(IEnumerable<BaseEntry> currentEntries)
    {
        var ordered = currentEntries.Order();
        
        return ordered.Select(entry => new FileSystemEntryDto(
                entry.Name,
                entry is File
                    ? FileSystemEntityType.File
                    : FileSystemEntityType.Directory,
                entry is File file
                    ? file.Version
                    : null
            ));
    }
    
    private static IEnumerable<FileSystemEntryDiffDto> MapDiffedEntries(FolderSnapshotDiff diff)
    {
        var ordered = diff.Entries.Order();
        
        return ordered.Select(entry => new FileSystemEntryDiffDto(
            entry.FileSystemEntry.Name,
            entry.FileSystemEntry is File
                ? FileSystemEntityType.File
                : FileSystemEntityType.Directory,
            entry.Operation,
            entry.FileSystemEntry is File file
                ? file.Version
                : null
        ));
    }

    // TODO find a common pattern to handle validation like this
    private bool ValidateRequest(string? path, out string fullPath, out string? error)
    {
        fullPath = Path.Combine(configuration.RootPublicPath, path ?? string.Empty);

        if (!browseService.IsPathValidAndBrowsable(fullPath))
        {
            error = "Path does not exist or is not accessible.";
            logger.LogWarning("{Error} Path: {Path}", error, fullPath);
            return false;
        }

        error = null;
        return true;       
    }
}
