using Moq;
using ServerFolderWatch.Core.Service.Interfaces;
using ServerFolderWatch.Desktop.Services;
using ServerFolderWatch.Desktop.ViewModels;
using ServerFolderWatch.Desktop.ViewModels.Items;
using File = ServerFolderWatch.Core.Model.File;

namespace ServerFolderWatch.Desktop.Tests;

public class FolderContentsViewModelTests
{
    [Fact]
    public void OpenEntryCommand_File_LaunchesFile()
    {
        var fileLauncherMock = new Mock<IFileLauncher>();
        var sut = new FolderContentsViewModel(
            Mock.Of<IFolderSnapshotService>(),
            Mock.Of<IFolderDiffService>(),
            Mock.Of<IBrowseService>(),
            Mock.Of<IDispatcherService>(),
            fileLauncherMock.Object);
        const string filePath = @"C:\files\report.txt";
        var file = new FileViewModel(new File("report.txt"), filePath);

        sut.OpenEntryCommand.Execute(file);

        fileLauncherMock.Verify(x => x.Open(filePath), Times.Once);
    }
}
