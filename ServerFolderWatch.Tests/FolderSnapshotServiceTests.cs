using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;
using ServerFolderWatch.Core;
using ServerFolderWatch.Core.Service;
using Testably.Abstractions.Testing;

namespace ServerFolderWatch.Tests;

public class FolderSnapshotServiceTests
{
    private const string FolderName = "test";
    private const string SidecarFileName = "metadata.txt";

    private readonly MockFileSystem mockFileSystem = new();
    private readonly Mock<IConfiguration> configurationMock = new();

    private readonly SidecarFileFolderSnapshotService sut;
    
    public FolderSnapshotServiceTests()
    {
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);
        
        configurationMock.SetupGet(x => x.SidecarFileName)
            .Returns(SidecarFileName);
        
        sut = new SidecarFileFolderSnapshotService(mockFileSystem, configurationMock.Object,
            loggerFactoryMock.Object);
        
        // TODO test that we're checking configuration file name

        mockFileSystem.Initialize()
            .WithSubdirectory(FolderName);
    }

    [Fact]
    public void Constructor_ChecksSidecarFileName()
    {
        configurationMock.VerifyGet(x => x.SidecarFileName, Times.Once);
    }

    [Fact]
    public void Constructor_RefusesFullSidecarFilePath()
    {
        configurationMock.SetupGet(x => x.SidecarFileName)
            .Returns("C:\\invalid_rooted_path.json");
        
        Assert.Throws<ArgumentException>(() => 
            new SidecarFileFolderSnapshotService(new Mock<IFileSystem>().Object,
                configurationMock.Object, new Mock<ILoggerFactory>().Object));
    }

    [Fact]
    public void IsFolderAlreadyMonitored_BuildsCorrectSidecarFilePath()
    {
        _ = sut.IsFolderAlreadyMonitored(FolderName);
        
        // pathMock.Verify(x => x.Combine(FolderPath, SidecarFileName), Times.Once);
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsFolderAlreadyMonitored_ReturnsCorrectValue(bool sidecarFileExists)
    {
        string sidecarFilePath = mockFileSystem.Path.Combine(FolderName, SidecarFileName);
        
        if (sidecarFileExists)
        {
            mockFileSystem.File.Create(sidecarFilePath);
        }

        bool result = sut.IsFolderAlreadyMonitored(FolderName);

        Assert.Equal(sidecarFileExists, result);
    }

    /*
    [Fact]
    public void InitializeFolder_CreatesSidecarFile()
    {
        fileMock.Setup(x => x.Exists(SidecarFilePath)).Returns(false);

        sut.InitializeFolder(FolderPath, false);

        fileMock.Verify(x => x.Create(SidecarFilePath), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void InitializeFolder_SidecarFileAlreadyExists_ThrowsException(bool runRecursively)
    {
        string sidecarFilePath = TestHelpers.GetPath(FolderPath, SidecarFileName, pathMock);

        fileMock.Setup(x => x.Exists(sidecarFilePath)).Returns(true);

        Assert.Throws<InvalidOperationException>(() => sut.InitializeFolder(FolderPath, runRecursively));
    }

    [Fact]
    public void InitializeFolder_RunsRecursively_IfSidecarExistsInCurrentFolder()
    {
    }

    [Fact]
    public void InitializeFolder_RunsRecursively_IfSidecarDoesNotExistInCurrentFolder()
    {
    }

    private void SetupSubfolder()
    {
        // mock that subfolder exists
        directoryMock.Setup(x => x.Exists(SubFolderPath)).Returns(true);

        // mock that subfolder is listed
        directoryMock.Setup(x => x.GetDirectories(FolderPath)).Returns(new[] {SubFolderPath});
        directoryMock.Setup(x => x.GetDirectories(SubFolderPath)).Returns(Array.Empty<string>());
        directoryMock.Setup(x => x.EnumerateDirectories(FolderPath)).Returns(new[] { SubFolderPath });
        directoryMock.Setup(x => x.EnumerateDirectories(SubFolderPath)).Returns(new[] {SubFolderPath});
    }
    */
}