using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;
using ServerFolderWatch.Core;
using ServerFolderWatch.Core.Service;

namespace ServerFolderWatch.Tests;

public class FolderSnapshotServiceTests
{
    private const string SidecarFileName = "metadata.txt";
    
    private readonly Mock<IPath> pathMock = new();
    private readonly Mock<IFile> fileMock = new();
    private readonly Mock<IDirectory> directoryMock = new();
    private readonly Mock<IConfiguration> configurationMock = new();
    
    private readonly SidecarFileFolderSnapshotService sut;
    
    private string FolderPath { get; }
    private string SubFolderPath { get; }
    private string SidecarFilePath { get; }
    private string SubSidecarFilePath { get; }
    
    public FolderSnapshotServiceTests()
    {
        FolderPath = TestHelpers.GetPath("foo");
        SidecarFilePath = TestHelpers.GetPath("foo", SidecarFileName, pathMock);
        SubFolderPath = TestHelpers.GetPath("foo", "bar", pathMock);
        SubSidecarFilePath = TestHelpers.GetPath("foo", "bar", SidecarFileName, pathMock);
        
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);

        var fileSystemMock = new Mock<IFileSystem>();
        fileSystemMock.SetupGet(x => x.File).Returns(fileMock.Object);
        fileSystemMock.SetupGet(x => x.Path).Returns(pathMock.Object);
        fileSystemMock.SetupGet(x => x.Directory).Returns(directoryMock.Object);
        
        configurationMock.SetupGet(x => x.SidecarFileName)
            .Returns(SidecarFileName);
        
        sut = new SidecarFileFolderSnapshotService(fileSystemMock.Object, configurationMock.Object,
            loggerFactoryMock.Object);
        
        // TODO test that we're checking configuration file name
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
        _ = sut.IsFolderAlreadyMonitored(FolderPath);
        
        pathMock.Verify(x => x.Combine(FolderPath, SidecarFileName), Times.Once);
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsFolderAlreadyMonitored_ReturnsCorrectValue(bool sidecarFileExists)
    {
        const string mockedSidecarFilePath = "mocked_sidecar_file_path";
            
            .Returns(mockedSidecarFilePath);
        fileMock.Setup(x => x.Exists(mockedSidecarFile))
            .Returns(sidecarFileExists);
        
        bool result = sut.IsFolderAlreadyMonitored(FolderPath);

        Assert.Equal(sidecarFileExists, result);
    }

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
}