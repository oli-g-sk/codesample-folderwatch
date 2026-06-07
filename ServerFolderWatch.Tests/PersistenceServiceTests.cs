using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;
using ServerFolderWatch.Core;
using ServerFolderWatch.Core.Service;

namespace ServerFolderWatch.Tests;

public class PersistenceServiceTests
{
    private const string FolderName = "foo";
    private const string SubFolderName = "bar";
    private const string SidecarFileName = "metadata.txt";
    
    private readonly Mock<IPath> pathMock = new();
    private readonly Mock<IFile> fileMock = new();
    private readonly Mock<IConfiguration> configurationMock = new();
    
    private readonly SidecarFilePersistenceService sut;

    public PersistenceServiceTests()
    {
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);

        var fileSystemMock = new Mock<IFileSystem>();
        fileSystemMock.SetupGet(x => x.Path).Returns(pathMock.Object);
        fileSystemMock.SetupGet(x => x.File).Returns(fileMock.Object);
        fileSystemMock.SetupGet(x => x.Path)
            .Returns(pathMock.Object);
        
        configurationMock.SetupGet(x => x.SidecarFileName)
            .Returns(SidecarFileName);
        
        sut = new SidecarFilePersistenceService(fileSystemMock.Object, configurationMock.Object,
            loggerFactoryMock.Object);
        
        // TODO test that we're checking configuration file name
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsFolderAlreadyMonitored_ReturnsCorrectValue(bool sidecarFileExists)
    {
        string sidecarFilePath = TestHelpers.GetPath(FolderName, SidecarFileName, pathMock);
        
        fileMock.Setup(x => x.Exists(sidecarFilePath))
            .Returns(sidecarFileExists);
        
        bool result = sut.IsFolderAlreadyMonitored(FolderName);

        Assert.Equal(sidecarFileExists, result);
    }

    [Fact]
    public void InitializeFolder_CreatesSidecarFile()
    {
        string sidecarFilePath = TestHelpers.GetPath(FolderName, SidecarFileName, pathMock);
        
        fileMock.Setup(x => x.Exists(sidecarFilePath)).Returns(false);
        
        sut.InitializeFolder(FolderName, false);
        
        fileMock.Verify(x => x.Create(sidecarFilePath), Times.Once);
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void InitializeFolder_SidecarFileAlreadyExists_ThrowsException(bool runRecursively)
    {
        string sidecarFilePath = TestHelpers.GetPath(FolderName, SidecarFileName, pathMock);
        
        fileMock.Setup(x => x.Exists(sidecarFilePath)).Returns(true);
        
        Assert.Throws<InvalidOperationException>(() => sut.InitializeFolder(FolderName, runRecursively));
    }
    
    [Fact]
    public void InitializeFolder_RunsRecursively_IfSidecarExistsInCurrentFolder()
    {
    }
    
    [Fact]
    public void InitializeFolder_RunsRecursively_IfSidecarDoesNotExistInCurrentFolder()
    {
    }
}