using Microsoft.AspNetCore.Mvc;

namespace ServerFolderWatch.Server.Controllers.WebControllers;

public class HealthController : Controller
{
    [HttpGet("/health")]
    public IActionResult Index() => Content("Running");
}
