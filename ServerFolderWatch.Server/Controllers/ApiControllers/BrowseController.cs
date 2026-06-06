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
    IPersistenceService persistenceService,
    IFolderDiffService diffService,
    IConfiguration configuration,
    ILoggerFactory loggerFactory) : ControllerBase
{
    private readonly ILogger<BrowseController> logger = loggerFactory.CreateLogger<BrowseController>();
    
    [Route("api/browse")]
    public IActionResult Browse([FromQuery(Name = "folder")] string? path)
    {
        if (!ValidateRequest(path, out var fullPath, out var error))
            return BadRequest(error);

        var currentContent = browseService.ListContents(fullPath);
        
        return Ok(new
        {
            LastAnalyzed = currentContent.LastAnalyzed,
            Entries = MapCurrentEntries(currentContent.GetAllEntries())
        });
    }
    
        
    [Route("api/diff")]
    public IActionResult Diff([FromQuery(Name = "folder")] string? path)
    {
        if (!ValidateRequest(path, out var fullPath, out var error))
            return BadRequest(error);

        var previousSnapshot = persistenceService.LoadSnapshot(fullPath);
        var currentContent = browseService.ListContents(fullPath);
        var diff = diffService.Diff(previousSnapshot, currentContent, fullPath);

        return Ok(new
        {
            LastAnalyzed = diffService.LastAnalyzed,
            Entries = MapDiffedEntries(diffService)
        });
    }

    private static IEnumerable<FileSystemEntryDto> MapCurrentEntries(IEnumerable<FileSystemEntry> currentEntries)
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
    
    private static IEnumerable<FileSystemEntryDiffDto> MapDiffedEntries(IMainService diffService)
    {
        var joinedEntries = diffService.AllEntries.Order();
        
        return joinedEntries.Select(entry => new FileSystemEntryDiffDto(
            entry.Name,
            entry is File
                ? FileSystemEntityType.File
                : FileSystemEntityType.Directory,
            GetDiffOperation(entry),
            entry is File file
                ? file.Version
                : null
        ));

        FileSystemEntityDiffOperation GetDiffOperation(FileSystemEntry entry)
        {
            if (diffService.AddedEntries.Contains(entry))
                return FileSystemEntityDiffOperation.Added;
            if (diffService.DeletedEntries.Contains(entry))
                return FileSystemEntityDiffOperation.Removed;
            if (diffService.ModifiedEntries.Contains(entry))
                return FileSystemEntityDiffOperation.Modified;
            return FileSystemEntityDiffOperation.Unchanged;
        }
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