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
    private readonly Mock<ILoggerFactory> loggerFactoryMock;

    private readonly BaseFolderSnapshotService sut;

    private string SidecarFilePath => mockFileSystem.Path.Combine(FolderName, SidecarFileName);
    
    public FolderSnapshotServiceTests()
    {
        loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);
        
        configurationMock.SetupGet(x => x.SidecarFileName)
            .Returns(SidecarFileName);
        
        sut = new JsonFileSnapshotService(mockFileSystem, configurationMock.Object,
            loggerFactoryMock.Object);
        
        // TODO test that we're checking configuration file name

        mockFileSystem.Initialize()
            .WithSubdirectory(FolderName);
    }

    [Fact]
    public void Constructor_ChecksSidecarFileName()
    {
        configurationMock.VerifyGet(x => x.SidecarFileName, Times.AtLeastOnce());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(@"foo/bar")]
    [InlineData("foo\\bar")]
    [InlineData("C:\\foo")]
    public void Constructor_LoadsConfiguration_RefusesInvalidFilename(string? sidecarFileName)
    {
        configurationMock.Reset();
        configurationMock.SetupGet(x => x.SidecarFileName)
            .Returns(sidecarFileName);
        
        Assert.Throws<ArgumentException>(() => 
            new JsonFileSnapshotService(mockFileSystem, configurationMock.Object,
                loggerFactoryMock.Object));
    }
    
    [Fact]
    public void Constructor_LoadsConfiguration_RefusesInvalidChars()
    {
        foreach (var invalidChar in mockFileSystem.Path.GetInvalidFileNameChars())
        {
            configurationMock.Reset();
            configurationMock.SetupGet(x => x.SidecarFileName)
                .Returns($"sidecar{invalidChar}");
            
            Assert.Throws<ArgumentException>(() => 
                new JsonFileSnapshotService(mockFileSystem, configurationMock.Object,
                    loggerFactoryMock.Object));
        }
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsFolderAlreadyMonitored_ReturnsCorrectValue(bool sidecarFileExists)
    {
        if (sidecarFileExists)
            mockFileSystem.File.Create(SidecarFilePath);

        bool result = sut.IsFolderAlreadyMonitored(FolderName);
        Assert.Equal(sidecarFileExists, result);
    }

    [Fact]
    public void InitializeFolder_CreatesSidecarFile()
    {
        sut.InitializeFolder(FolderName, false);
        Assert.True(mockFileSystem.File.Exists(SidecarFilePath));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void InitializeFolder_SidecarFileAlreadyExists_ThrowsException(bool runRecursively)
    {
        mockFileSystem.File.Create(SidecarFilePath);
        Assert.Throws<InvalidOperationException>(() => sut.InitializeFolder(FolderName, runRecursively));
    }

    [Fact]
    public void InitializeFolder_NotInitializedBefore_RunsRecursively()
    {
        mockFileSystem.File.Create(SidecarFilePath);
    }
    
    [Fact]
    public void InitializeFolder_WasnitializedBefore_DoesNotRunRecursively()
    {
        mockFileSystem.File.Create(SidecarFilePath);
    }

    [Fact]
    public void InitializeFolder_WasInitializedBefore_RunsOnNewSubfolder()
    {
    }
}