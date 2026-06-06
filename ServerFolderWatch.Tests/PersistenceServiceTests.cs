using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;
using ServerFolderWatch.Core;
using ServerFolderWatch.Core.Service;

namespace ServerFolderWatch.Tests;

public class PersistenceServiceTests
{
    private const string FolderName = "foo";
    private const string SidecarFileName = "bar.txt";
    private static string SidecarFilePath => CombinePaths(FolderName, SidecarFileName);
    
    private readonly Mock<IPath> pathMock = new();
    private readonly Mock<IFile> fileMock = new();
    private readonly Mock<IConfiguration> configurationMock = new();
    
    private readonly SidecarFilePersistenceService sut;

    public PersistenceServiceTests()
    {
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);
        
        pathMock.Setup(x => x.Combine(FolderName, SidecarFileName))
            .Returns(SidecarFilePath);

        var fileSystemMock = new Mock<IFileSystem>();
        fileSystemMock.SetupGet(x => x.Path).Returns(pathMock.Object);
        fileSystemMock.SetupGet(x => x.File).Returns(fileMock.Object);
        fileSystemMock.SetupGet(x => x.Path)
            .Returns(pathMock.Object);
        
        configurationMock.SetupGet(x => x.SidecarFileName)
            .Returns(SidecarFileName);
        
        sut = new SidecarFilePersistenceService(fileSystemMock.Object, configurationMock.Object,
            loggerFactoryMock.Object);
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsFolderAlreadyMonitored_ReturnsCorrectValue(bool sidecarFileExists)
    {
        fileMock.Setup(x => x.Exists(SidecarFilePath)).Returns(sidecarFileExists);
        
        bool result = sut.IsFolderAlreadyMonitored(FolderName);

        Assert.Equal(sidecarFileExists, result);
    }

    [Fact]
    public void InitializeFolder_CreatesSidecarFile()
    {
        fileMock.Setup(x => x.Exists(SidecarFilePath)).Returns(false);
        
        sut.InitializeFolder(FolderName);
        
        fileMock.Verify(x => x.Create(SidecarFilePath), Times.Once);
    }

    [Fact]
    public void InitializeFolder_SidecarFileAlreadyExists_ThrowsException()
    {
        fileMock.Setup(x => x.Exists(SidecarFilePath)).Returns(true);
        
        Assert.Throws<InvalidOperationException>(() => sut.InitializeFolder(FolderName));
    }
    
    // TODO duplicate code
    private static string CombinePaths(string part1, string part2)
    {
        return $"filesystem://{part1}/{part2}";
    }
}