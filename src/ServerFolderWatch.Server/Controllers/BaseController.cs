using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core.Service.Interfaces;

namespace ServerFolderWatch.Server.Controllers;

[ApiController]
public abstract class BaseController(
    IBrowseService browseService,
    ILoggerFactory loggerFactory)
    : ControllerBase
{
    private readonly ILogger<BaseController> logger = loggerFactory.CreateLogger<BaseController>();

    protected bool ValidateRequest(string path, out IActionResult? errorResult)
    {
        if (!browseService.FolderExists(path))
        {
            var error = $"Path does not exist";
            logger.LogWarning("{Error}: {Path}", error, path);
            errorResult = NotFound(error);
            return false;
        }
        
        if (!browseService.CanReadFolderContents(path))
        {
            var error = "Cannot read folder contents.";
            logger.LogWarning("{Error} Path: {Path}", error, path);
            errorResult = Forbid(error);
            return false;
        }
        
        errorResult = null;
        return true;       
    }
}