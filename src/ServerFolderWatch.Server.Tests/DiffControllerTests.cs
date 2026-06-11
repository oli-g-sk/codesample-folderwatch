using Microsoft.AspNetCore.Mvc;
using ServerFolderWatch.Server.Controllers;

namespace ServerFolderWatch.Server.Tests;

public class DiffControllerTests : PathScopedControllersTests<DiffController>
{
    protected override DiffController CreateController()
    {
        return new DiffController(BrowseServiceMock.Object, FolderSnapshotServiceMock.Object,
            FolderDiffServiceMock.Object, LoggerFactoryMock.Object);
    }
    
    protected override IEnumerable<ControllerDelegate> Endpoints { get; } = new List<ControllerDelegate>
    {
        DiffEndpoint
    };

    private static IActionResult DiffEndpoint(DiffController controller, string? path)
        => controller.Diff(path);
}