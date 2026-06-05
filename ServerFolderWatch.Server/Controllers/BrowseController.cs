using System.IO;
using Microsoft.AspNetCore.Mvc;
using ServerFolderWatch.Core.Service;

namespace ServerFolderWatch.Server.Controllers;

[ApiController]
[Route("api/browse")]
public class BrowseController(IBrowseService browseService) : ControllerBase
{
    public IActionResult Index() => Ok(browseService.ListContents(Directory.GetCurrentDirectory()));
}