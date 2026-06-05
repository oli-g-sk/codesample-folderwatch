using System;
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

namespace ServerFolderWatch.Server.Controllers;

[ApiController]
[Route("api/browse")]
public class BrowseController(IBrowseService browseService,
    IFileSystemDiffService diffService,
    IConfiguration configuration,
    ILoggerFactory loggerFactory) : ControllerBase
{
    private readonly ILogger<BrowseController> logger = loggerFactory.CreateLogger<BrowseController>();
    
    public IActionResult Browse([FromQuery(Name = "folder")] string? path)
    {
        var fullPath = Path.Combine(configuration.RootPublicPath, path ?? string.Empty);

        if (!browseService.IsPathValidAndBrowsable(fullPath))
        {
            const string message = "Path does not exist or is not accessible.";
            logger.LogWarning("{Error} Path: {Path}", message, fullPath);
            return BadRequest(message);
        }

        diffService.Analyze(fullPath).Wait();

        // TODO add path validation and throw 500 if invalid

        var currentFiles = diffService.CurrentEntries;
        
        return Ok(new
        {
            LastAnalyzed = diffService.LastAnalyzed,
            Entries = MapEntries(currentFiles)
        });
    }

    private static IEnumerable<FileSystemEntryDto> MapEntries(IEnumerable<FileSystemEntry> fileSystemEntries)
    {
        var ordered = fileSystemEntries.Order();
        
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
}