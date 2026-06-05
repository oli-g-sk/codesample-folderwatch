using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ServerFolderWatch.Core;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service;
using ServerFolderWatch.Server.DTOs;

namespace ServerFolderWatch.Server.Controllers;

[ApiController]
[Route("api/browse")]
public class BrowseController(IBrowseService browseService,
    IFileSystemDiffService diffService,
    IConfiguration configuration) : ControllerBase
{
    public IActionResult Browse([FromQuery(Name = "folder")] string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            path = configuration.RootPublicPath;

        diffService.Analyze(path).Wait();

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