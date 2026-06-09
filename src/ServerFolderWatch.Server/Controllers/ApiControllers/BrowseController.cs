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
    IAppConfiguration configuration,
    ILoggerFactory loggerFactory) : ControllerBase
{
    private readonly ILogger<BrowseController> logger = loggerFactory.CreateLogger<BrowseController>();
    
    [HttpGet("api/browse")]
    public IActionResult Browse([FromQuery(Name = "folder")] string? path)
    {
        // TODO is it okay to coerce this here?
        path ??= configuration.RootPublicPath;
        
        if (!ValidateRequest(path, out var error))
            return error!;

        var currentContent = folderSnapshotService.GetCurrentContents(path);
        
        return Ok(new
        {
            Path = path,
            Entries = MapCurrentEntries(currentContent.GetAllEntries())
        });
    }
        
    [HttpGet("api/diff")]
    public IActionResult Diff([FromQuery(Name = "folder")] string? path)
    {
        // TODO is it okay to coerce this here?
        path ??= configuration.RootPublicPath;
        
        if (!ValidateRequest(path, out var error))
            return error!;

        var previousSnapshot =
            folderSnapshotService.IsFolderAlreadyMonitored(path) ?
                folderSnapshotService.LoadPersistedSnapshot(path)
                : FolderSnapshot.Empty;

        var currentContents = folderSnapshotService.GetCurrentContents(path);
        var diff = folderDiffService.Compare(previousSnapshot, currentContents, path, out _);

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
    private bool ValidateRequest(string path, out IActionResult? errorResult)
    {
        if (!browseService.FolderExists(path))
        {
            var error = $"Path does not exist";
            logger.LogWarning("{Error}: {Path}", error, path);
            errorResult = NotFound(error);
            return false;
        }
        
        if (!browseService.CanReadFolderContents(path))
        {
            var error = "Cannot read folder contents.";
            logger.LogWarning("{Error} Path: {Path}", error, path);
            errorResult = Forbid(error);
            return false;
        }
        
        errorResult = null;
        return true;       
    }
}
