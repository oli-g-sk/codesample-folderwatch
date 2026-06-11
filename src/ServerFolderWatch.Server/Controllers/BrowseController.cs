using System.Collections.Generic;
using System.Linq;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Server.DTOs;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Server.Controllers;

[ApiController]
public class BrowseController(IBrowseService browseService,
    IFolderSnapshotService folderSnapshotService,
    IFolderDiffService folderDiffService,
    IAppConfiguration configuration,
    ILoggerFactory loggerFactory)
    : PathControllerBase(browseService, loggerFactory)
{
    private readonly ILogger<BrowseController> logger = loggerFactory.CreateLogger<BrowseController>();
    
    [HttpGet("api/browse")]
    public IActionResult Browse([FromQuery(Name = "folder")] string? path)
    {
        path = CoercePath(path);
        
        if (!ValidateRequest(path, out var error))
            return error!;

        var currentContent = folderSnapshotService.GetCurrentContents(path);
        var entries = currentContent.GetAllEntries().Order();
        
        return Ok(new BrowseResponseDto()
        {
            Path = path,
            Contents = entries.Select(x => x.Adapt<FileSystemEntryDto>()),
        });
    }
}
