using Moq;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Desktop.Services;
using ServerFolderWatch.Desktop.ViewModels.Items;

namespace ServerFolderWatch.Desktop.Tests;

public class FolderViewModelTests
{
    [Fact]
    public void Icon_NotExpanded_ReturnsClosedFolderIcon()
    {
        var sut = CreateSut(isExpanded: false);

        Assert.Equal("📁", sut.Icon);
    }

    [Fact]
    public void Icon_Expanded_ReturnsOpenFolderIcon()
    {
        var sut = CreateSut(isExpanded: true);

        Assert.Equal("📂", sut.Icon);
    }

    [Fact]
    public void Constructor_IsMonitored_SetsIsMonitored()
    {
        var sut = CreateSut(isMonitored: true);

        Assert.True(sut.IsMonitored);
    }

    private static FolderViewModel CreateSut(bool isExpanded = false, bool isMonitored = false)
    {
        var sut = new FolderViewModel(
            new Folder("docs"),
            @"C:\docs",
            hasChildren: false,
            canViewContents: true,
            isMonitored,
            Mock.Of<IDispatcherService>());
        sut.IsExpanded = isExpanded;
        return sut;
    }
}
