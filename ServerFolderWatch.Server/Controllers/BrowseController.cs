using Microsoft.AspNetCore.Mvc;

namespace ServerFolderWatch.Server.Controllers;

[ApiController]
[Route("api/browse")]
public class BrowseController : ControllerBase
{
    public IActionResult Index() => Ok("Hello World");
}