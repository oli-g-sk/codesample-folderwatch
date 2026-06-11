using Microsoft.AspNetCore.Mvc;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Server.Controllers;
using ServerFolderWatch.Server.DTOs;
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
        Assert.IsType<BrowseResponseDto>(ok.Value);
        
        var dto = (BrowseResponseDto) ok.Value;
        Assert.Equal(path, dto.Path);
    }
}