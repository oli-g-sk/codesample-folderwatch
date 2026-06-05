using System;
using Microsoft.AspNetCore.Mvc;

namespace ServerFolderWatch.Server.Controllers.ApiControllers;

[ApiController]
[Route("api")]
public class MainController : ControllerBase
{
    [HttpGet]
    public IActionResult Index() => Ok("Hello World");
    
    [HttpGet("health")]
    public IActionResult Health()
    {
        var health = new
        {
            status = "Running",
            time = DateTime.Now
        };
        
        return Ok(health);
    }
}