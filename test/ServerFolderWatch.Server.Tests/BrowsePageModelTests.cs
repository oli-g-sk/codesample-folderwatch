using Moq;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Server.PageModels;

namespace ServerFolderWatch.Server.Tests;

public class BrowsePageModelTests
{
    private readonly Mock<IFolderSnapshotService> folderSnapshotServiceMock = new();
    private readonly BrowsePageModel sut;

    public BrowsePageModelTests()
    {
        sut = new BrowsePageModel(
            folderSnapshotServiceMock.Object,
            Mock.Of<IFolderDiffService>(),
            Mock.Of<IBrowseService>());
    }

    [Fact]
    public void GetBreadcrumbs_LoadedFolder_ReturnsBreadcrumbViewModels()
    {
        folderSnapshotServiceMock.Setup(x => x.IsFolderAlreadyMonitored("foo/bar"))
            .Returns(true);
        sut.LoadFolder("foo/bar");

        var result = sut.GetBreadcrumbs().ToList();

        Assert.Collection(result,
            breadcrumb =>
            {
                Assert.Equal("foo", breadcrumb.Name);
                Assert.Equal("📂", breadcrumb.Icon);
                Assert.False(breadcrumb.IsMonitored);
            },
            breadcrumb =>
            {
                Assert.Equal("bar", breadcrumb.Name);
                Assert.Equal("/browse?folder=foo%2Fbar", breadcrumb.Url);
                Assert.Equal("📂", breadcrumb.Icon);
                Assert.True(breadcrumb.IsMonitored);
            });
    }

    [Fact]
    public void IsFolderMonitored_MonitoredFolder_ReturnsTrue()
    {
        folderSnapshotServiceMock.Setup(x => x.IsFolderAlreadyMonitored("foo/bar"))
            .Returns(true);
        sut.LoadFolder("foo");

        bool result = sut.IsFolderMonitored(new Folder("bar"));

        Assert.True(result);
    }

    [Fact]
    public void GetFolderIcon_ChildFolder_ReturnsClosedFolderIcon()
    {
        sut.LoadFolder("foo");

        string result = sut.GetFolderIcon(new Folder("bar"));

        Assert.Equal("📁", result);
    }
}
