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
    
        
    [Fact]
    public void Browse_ValidPath_ReturnsFolderContents()
    {
        string path = "some/path";
        
        var expected = new FolderSnapshot()
        {
            Subfolders = { new Folder("foo") },
            VersionedFiles = { new File("bar") }
        };
            
        SetupFolder(path);
        
        FolderSnapshotServiceMock.Setup(x => x.GetCurrentContents(path))
            .Returns(expected);
        
        var sut = CreateController();
        var result = sut.Browse(path);
        
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, ok.Value);
    }
}