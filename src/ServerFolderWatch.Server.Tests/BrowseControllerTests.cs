using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ServerFolderWatch.Core;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Server.Controllers;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Server.Tests;

public class BrowseControllerTests
{
    private const string ConfiguredRootPath = "M:\\Public";
    
    private readonly Mock<IBrowseService> browseServiceMock = new();
    private readonly Mock<IFolderSnapshotService> folderSnapshotServiceMock = new();
    private readonly Mock<IFolderDiffService> folderDiffServiceMock = new();
    private readonly Mock<IAppConfiguration> configurationMock = new();
    private readonly Mock<ILoggerFactory> loggerFactoryMock = new();
    
    private readonly BrowseController sut;
    
    public BrowseControllerTests()
    {
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);
        configurationMock.SetupGet(x => x.RootPublicPath)
            .Returns(ConfiguredRootPath);
        configurationMock.SetupGet(x => x.SidecarFileName)
            .Returns(".sidecar.json");
        
        sut = new BrowseController(browseServiceMock.Object,
            folderSnapshotServiceMock.Object, folderDiffServiceMock.Object,
            configurationMock.Object, loggerFactoryMock.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Browse_NoParameter_ListsConfiguredRoot(string? path)
    {
        string rootFolderRelativePath = string.Empty;

        browseServiceMock.Setup(x => x.FolderExists(rootFolderRelativePath))
            .Returns(true);
        browseServiceMock.Setup(x => x.CanReadFolderContents(rootFolderRelativePath))
            .Returns(true);
        folderSnapshotServiceMock.Setup(x => x.GetCurrentContents(rootFolderRelativePath))
            .Returns(new FolderSnapshot());
        
        var result = sut.Browse(path);
        
        folderSnapshotServiceMock.Verify(x =>
            x.GetCurrentContents(rootFolderRelativePath), Times.Once);
        
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public void Browse_ValidPath_ReturnsFolderContents()
    {

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