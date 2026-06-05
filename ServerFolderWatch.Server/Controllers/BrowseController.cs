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
public class BrowseController(IBrowseService browseService, IConfiguration configuration) : ControllerBase
{
    public IActionResult Index()
    {
        var contents = browseService.ListContents(configuration.RootPublicPath);
        return Ok(new
        {
            LastAnalyzed = contents.LastAnalyzed,
            Entries = MapEntries(contents)
        });
    }

    private IEnumerable<FileSystemEntryDto> MapEntries(FolderContents folderContents)
    {
        var ordered = folderContents.GetAllEntries()
            .Order();
        
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