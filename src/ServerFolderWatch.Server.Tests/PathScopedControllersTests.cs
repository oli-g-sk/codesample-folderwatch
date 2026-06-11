using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ServerFolderWatch.Core;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Server.Controllers;

namespace ServerFolderWatch.Server.Tests;

public abstract class PathScopedControllersTests<T>
    where T : BaseController
{
    private const string ConfiguredRootPath = "M:\\Public";
    
    protected readonly Mock<IBrowseService> BrowseServiceMock = new();
    protected readonly Mock<IFolderSnapshotService> FolderSnapshotServiceMock = new();
    protected readonly Mock<IFolderDiffService> FolderDiffServiceMock = new();
    protected readonly Mock<IAppConfiguration> ConfigurationMock = new();
    protected readonly Mock<ILoggerFactory> LoggerFactoryMock = new();
    
    public PathScopedControllersTests()
    {
        LoggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);
        ConfigurationMock.SetupGet(x => x.RootPublicPath)
            .Returns(ConfiguredRootPath);
        ConfigurationMock.SetupGet(x => x.SidecarFileName)
            .Returns(".sidecar.json");
    }

    protected abstract T CreateController();
    
    protected delegate IActionResult ControllerDelegate(T controller, string? path);
    
    protected abstract IEnumerable<ControllerDelegate> Endpoints { get; }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Browse_NoParameter_ListsConfiguredRoot(string? path)
    {
        string rootFolderRelativePath = string.Empty;
        
        BrowseServiceMock.Setup(x => x.FolderExists(path))
            .Returns(true);
        BrowseServiceMock.Setup(x => x.CanReadFolderContents(path))
            .Returns(true);
        
        FolderSnapshotServiceMock.Setup(x => x.GetCurrentContents(rootFolderRelativePath))
            .Returns(new FolderSnapshot());
        
        var sut = CreateController();

        foreach (var endpoint in Endpoints)
        {
            var result = endpoint.Invoke(sut, path);

            BrowseServiceMock.Verify(x => x.FolderExists(rootFolderRelativePath), Times.Once);
            BrowseServiceMock.Verify(x => x.CanReadFolderContents(rootFolderRelativePath), Times.Once);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }
    }

    [Theory]
    [InlineData("foo<bar")]
    [InlineData("foo>bar")]
    [InlineData("foo|bar")]
    [InlineData("foo:bar")]
    public void Browse_InvalidPath_ReturnsBadRequest(string path)
    {
        // TODO implement BadRequest response for malformed paths
    }
    
    [Fact]
    public void Browse_PathDoesNotExist_ReturnsNotFound()
    {
        string validPath = "foo";
        string nonExistentPath = "foo/bar";
        
        BrowseServiceMock.Setup(x => x.FolderExists(validPath))
            .Returns(true);
        BrowseServiceMock.Setup(x => x.CanReadFolderContents(validPath))
            .Returns(true);
        BrowseServiceMock.Setup(x => x.FolderExists(nonExistentPath))
            .Returns(false);
        
        var sut = CreateController();
        
        foreach (var endpoint in Endpoints)
            Assert.IsType<NotFoundObjectResult>(endpoint.Invoke(sut, nonExistentPath));
    }

    [Theory]
    [InlineData("../")]
    [InlineData("foo/../bar")]
    [InlineData("/etc")]
    [InlineData("C:\\")]
    public void Browse_PathOutsideRoot_ReturnsUnauthorized(string path)
    {
        BrowseServiceMock.Setup(x => x.FolderExists(path))
            .Returns(true);
        BrowseServiceMock.Setup(x => x.CanReadFolderContents(path))
            .Returns(false);
        
        var sut = CreateController();
        
        foreach (var endpoint in Endpoints)
            Assert.IsType<ForbidResult>(endpoint.Invoke(sut, path));
    }
}