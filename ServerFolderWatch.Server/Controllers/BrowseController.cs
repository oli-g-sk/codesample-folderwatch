using System.IO;
using Microsoft.AspNetCore.Mvc;
using ServerFolderWatch.Core;
using ServerFolderWatch.Core.Service;

namespace ServerFolderWatch.Server.Controllers;

[ApiController]
[Route("api/browse")]
public class BrowseController(IBrowseService browseService, IConfiguration configuration) : ControllerBase
{
    public IActionResult Index()
    {
        return Ok(browseService.ListContents(configuration.DefaultPath));
    }
}