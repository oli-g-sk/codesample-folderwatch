using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Server.DTOs;

namespace ServerFolderWatch.Server.Controllers;

[ApiController]
public abstract class PathControllerBase(
    IBrowseService browseService,
    ILoggerFactory loggerFactory)
    : ControllerBase
{
    private readonly ILogger<PathControllerBase> logger = loggerFactory.CreateLogger<PathControllerBase>();

    protected bool ValidateRequest(string path, out IActionResult? errorResult)
    {
        if (!browseService.FolderExists(path))
        {
            var error = $"Path does not exist";
            logger.LogWarning("{Error}: {Path}", error, path);
            errorResult = NotFound(new PathErrorResponseDto(path, error));
            return false;
        }
        
        if (!browseService.CanReadFolderContents(path))
        {
            var error = "Cannot read folder contents.";
            logger.LogWarning("{Error} Path: {Path}", error, path);
            
            // TODO 403 may be more fitting here, but it doesn't have a ctor for arbitrary response object
            errorResult = Unauthorized(new PathErrorResponseDto(path, error));
            return false;
        }
        
        errorResult = null;
        return true;       
    }
    
    protected static string CoercePath(string? path)
    {
        return path == null ? string.Empty : path.Trim();
    }
}