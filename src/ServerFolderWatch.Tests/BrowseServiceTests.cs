using System.IO.Abstractions;
using Moq;
using ServerFolderWatch.Core;
using ServerFolderWatch.Core.Service;
using Testably.Abstractions;

namespace ServerFolderWatch.Tests;

public class BrowseServiceTests
{
    // TODO add tests for sorting of returned items

    [Fact]
    public void GetFileSystemPath_AnchorsRelativePathToConfiguredPublicRoot()
    {
        var fileSystem = new RealFileSystem();
        var rootFolderName = $"browse-service-{Guid.NewGuid():N}";
        var rootPath = Path.Combine(AppContext.BaseDirectory, rootFolderName);
        var childPath = Path.Combine(rootPath, "child");
        Directory.CreateDirectory(childPath);

        try
        {
            var sut = CreateSut(rootFolderName, fileSystem);

            var result = sut.GetFileSystemPath("child");

            Assert.Equal(childPath, result);
        }
        finally
        {
            Directory.Delete(rootPath, true);
        }
    }

    [Fact]
    public void GetFileSystemPath_UsesPublicRootForEmptyPath()
    {
        var fileSystem = new RealFileSystem();
        var rootFolderName = $"browse-service-{Guid.NewGuid():N}";
        var rootPath = Path.Combine(AppContext.BaseDirectory, rootFolderName);
        Directory.CreateDirectory(rootPath);

        try
        {
            var sut = CreateSut(rootFolderName, fileSystem);

            var result = sut.GetFileSystemPath(string.Empty);

            Assert.Equal(rootPath, result);
        }
        finally
        {
            Directory.Delete(rootPath, true);
        }
    }

    [Fact]
    public void GetFileSystemPath_RejectsRelativePathOutsidePublicRoot()
    {
        var sut = CreateSut("public", new RealFileSystem());

        Assert.Throws<UnauthorizedAccessException>(() => sut.GetFileSystemPath("../private"));
    }

    private void FromFolder_ExcludesSidecarFile()
    {
    }

    private static BrowseService CreateSut(string rootPublicPath, IFileSystem fileSystem)
    {
        var configurationMock = new Mock<IAppConfiguration>();
        configurationMock.SetupGet(x => x.RootPublicPath)
            .Returns(rootPublicPath);
        configurationMock.SetupGet(x => x.SidecarFileName)
            .Returns(".sidecar.json");

        return new BrowseService(configurationMock.Object, fileSystem);
    }
}
