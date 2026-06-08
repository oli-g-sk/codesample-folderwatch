using Microsoft.Extensions.Logging;
using Moq;
using ServerFolderWatch.Core;
using ServerFolderWatch.Core.Service;
using ServerFolderWatch.Core.Service.Interfaces;
using Testably.Abstractions.Testing;

namespace ServerFolderWatch.Tests;

public class JsonFolderSnapshotServiceTests
{
    private const string FolderName = "test";
    private const string SidecarFileName = "metadata.txt";
    
    private readonly MockFileSystem mockFileSystem = new();
    private readonly Mock<BrowseService> browseServiceMock = new();
    private readonly Mock<IAppConfiguration> configurationMock = new();
    private readonly Mock<ILoggerFactory> loggerFactoryMock;

    private readonly BaseFolderSnapshotService sut;

    private string SidecarFilePath => mockFileSystem.Path.Combine(FolderName, SidecarFileName);
    
    public JsonFolderSnapshotServiceTests()
    {
        loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);
        
        configurationMock.SetupGet(x => x.SidecarFileName)
            .Returns(SidecarFileName);
        
        sut = new JsonFolderSnapshotService(browseServiceMock.Object,
            mockFileSystem, configurationMock.Object, loggerFactoryMock.Object);
        
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
            new JsonFolderSnapshotService(browseServiceMock.Object, mockFileSystem, configurationMock.Object,
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
                new JsonFolderSnapshotService(browseServiceMock.Object, mockFileSystem, configurationMock.Object,
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
    public void InitializeFolder_NotInitializedBefore_CreatesSidecarFile()
    {
        bool result = sut.InitializeFolder(FolderName, false);
        
        Assert.True(result);
        Assert.True(mockFileSystem.File.Exists(SidecarFilePath));
    }
    
    [Fact]
    public void InitializeFolder_WasInitializedBefore_DoesNothing()
    {
        // Prepare a sidecar file with dummy content
        string tempFileContents = DateTime.Now.Ticks.ToString();
        mockFileSystem.File.WriteAllText(SidecarFilePath, tempFileContents);
        
        bool result = sut.InitializeFolder(FolderName, false);
        
        // Verify sidecar file was not overwritten
        Assert.False(result);
        string content = mockFileSystem.File.ReadAllText(SidecarFilePath);
        Assert.Equal(tempFileContents, content);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]   
    public void InitializeFolder_RunsRecursively(bool currentFolderWasInitializedBefore)
    {
        if (currentFolderWasInitializedBefore)
            mockFileSystem.File.Create(SidecarFilePath);
        
        const string subfolder1 = "subfolder1";
        const string subfolder2 = "subfolder2";

        mockFileSystem.Directory.CreateDirectory(mockFileSystem.Path.Combine(FolderName, subfolder1));
        mockFileSystem.Directory.CreateDirectory(mockFileSystem.Path.Combine(FolderName, subfolder2));
        
        sut.InitializeFolder(FolderName, true);

        string subfolderSidecarFile1 = mockFileSystem.Path.Combine(FolderName, subfolder1, SidecarFileName);
        Assert.True(mockFileSystem.File.Exists(subfolderSidecarFile1));
        string subfolderSidecarFile2 = mockFileSystem.Path.Combine(FolderName, subfolder2, SidecarFileName);
        Assert.True(mockFileSystem.File.Exists(subfolderSidecarFile2));
    }
}