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
        SetupFolder(rootFolderRelativePath);
        
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

    protected void SetupFolder(string? path)
    {
        BrowseServiceMock.Setup(x => x.FolderExists(path))
            .Returns(true);
        BrowseServiceMock.Setup(x => x.CanReadFolderContents(path))
            .Returns(true);
    }

    [Fact]
    public void Browse_InvalidPath_ReturnsBadRequest()
    {
    }
    
    [Fact]
    public void Browse_PathDoesNotExist_ReturnsNotFound()
    {
        
    }

    [Fact]
    public void Browse_PathOutsideRoot_ReturnsUnauthorized()
    {
        
    }
}