using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ServerFolderWatch.Core;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Server.Controllers;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Server.Tests;

public class BrowseControllerTests : PathScopedControllersTests<BrowseController>
{
    protected override BrowseController CreateController()
    {
        return new BrowseController(BrowseServiceMock.Object, FolderSnapshotServiceMock.Object,
            FolderDiffServiceMock.Object, ConfigurationMock.Object, LoggerFactoryMock.Object);
    }

    protected override IEnumerable<ControllerDelegate> Endpoints { get; } = new List<ControllerDelegate>
    {
        BrowseEndpoint
    };

    private static IActionResult BrowseEndpoint(BrowseController controller, string? path)
        => controller.Browse(path);
}