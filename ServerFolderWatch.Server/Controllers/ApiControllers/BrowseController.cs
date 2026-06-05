using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service;
using ServerFolderWatch.Server.DTOs;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Server.Controllers.ApiControllers;

[ApiController]
public class BrowseController(IBrowseService browseService,
    IFileSystemDiffService diffService,
    IConfiguration configuration,
    ILoggerFactory loggerFactory) : ControllerBase
{
    private readonly ILogger<BrowseController> logger = loggerFactory.CreateLogger<BrowseController>();
    
    [Route("api/browse")]
    public IActionResult Browse([FromQuery(Name = "folder")] string? path)
    {
        var fullPath = Path.Combine(configuration.RootPublicPath, path ?? string.Empty);

        if (!browseService.IsPathValidAndBrowsable(fullPath))
        {
            const string message = "Path does not exist or is not accessible.";
            logger.LogWarning("{Error} Path: {Path}", message, fullPath);
            return BadRequest(message);
        }

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
        var fullPath = Path.Combine(configuration.RootPublicPath, path ?? string.Empty);

        if (!browseService.IsPathValidAndBrowsable(fullPath))
        {
            const string message = "Path does not exist or is not accessible.";
            logger.LogWarning("{Error} Path: {Path}", message, fullPath);
            return BadRequest(message);
        }

        diffService.Analyze(fullPath).Wait();

        var currentFiles = diffService.CurrentEntries;
        
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
    
    private static IEnumerable<FileSystemEntryDiffDto> MapDiffedEntries(IFileSystemDiffService diffService)
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
}