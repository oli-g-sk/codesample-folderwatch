using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;
using ServerFolderWatch.Core;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service;

namespace ServerFolderWatch.Tests;

public class FileSystemDiffServiceTests
{
    private const string FolderName = "foo";
    private const string SubFolderName = "subFolder";
    
    // TODO remove? (belong to persistence service)
    private const string SidecarFileName = "bar.txt";
    private static string SidecarFilePath => CombinePaths(FolderName, SidecarFileName);

    private static string SubFolderPath => CombinePaths(FolderName, SubFolderName);
    
    private readonly Mock<IPath> pathMock = new();
    private readonly Mock<IDirectory> directoryMock = new();
    private readonly Mock<IFile> fileMock = new();
    private readonly Mock<IConfiguration> configurationMock = new();
    
    private readonly Mock<IBrowseService> browseServiceMock1;
    private readonly Mock<IPersistenceService> persistenceServiceMock;
    
    private readonly FileSystemDiffService sut;

    public FileSystemDiffServiceTests()
    {
        browseServiceMock1 = new Mock<IBrowseService>();
        browseServiceMock1.Setup(x => x.ListContents(It.IsAny<string>()))
            .Returns(FolderContents.Empty);
        
        persistenceServiceMock = new Mock<IPersistenceService>();
        
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);
        
        var fileSystemMock = new Mock<IFileSystem>();
        fileSystemMock.SetupGet(x => x.Directory).Returns(directoryMock.Object);
        fileSystemMock.SetupGet(x => x.Path).Returns(pathMock.Object);
        fileSystemMock.SetupGet(x => x.File).Returns(fileMock.Object);
        
        configurationMock.SetupGet(x => x.SidecarFileName)
            .Returns(SidecarFileName);
        pathMock.Setup(x => x.Combine(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(SidecarFilePath);
        
        sut = new FileSystemDiffService(fileSystemMock.Object, browseServiceMock1.Object,
            persistenceServiceMock.Object, loggerFactoryMock.Object);
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Setup_ReturnsCorrectValue(bool sidecarFileExists)
    {
        const string folder = "myFolder";
        directoryMock.Setup(x => x.Exists(It.IsAny<string>()))
            .Returns(true);
        
        fileMock.Setup(x => x.Exists(SidecarFilePath))
            .Returns(sidecarFileExists);
        
        bool actual = await sut.Analyze("foo");
        Assert.Equal(sidecarFileExists, actual);
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Setup_NewFolder_InitializesIfNeeded(bool folderWasInitialized)
    {
        persistenceServiceMock.Setup(x => x.IsFolderAlreadyMonitored(FolderName))
            .Returns(folderWasInitialized);
        
        _ = await sut.Analyze("foo");
        
        Times timesToCallInitialization = folderWasInitialized ? Times.Never() : Times.Once();
        persistenceServiceMock.Verify(x => x.InitializeFolder(FolderName),timesToCallInitialization);
    }

    [Fact]
    public async Task Setup_NewFolder_RunsRecursively()
    {
        // mock there's no sidecar file in folder
        fileMock.Setup(x => x.Exists(SidecarFilePath)).Returns(false);
        
        SetupSubfolder();
        
        // mock there's no sidecar file in subfolder
        var subSidecarFilePath = CombinePaths(SubFolderPath, SidecarFileName);
        fileMock.Setup(x => x.Exists(subSidecarFilePath)).Returns(false);
        
        // setup path combining for subfolder sidecar file
        pathMock.Setup(x => x.Combine(SubFolderName, SidecarFileName))
            .Returns(subSidecarFilePath);
        
        _ = await sut.Analyze(FolderName);
        
        // verify a sidecar file was created in subfolder
        fileMock.Verify(x => x.Create(subSidecarFilePath), Times.Once);
    }

    [Fact]
    public async Task Setup_ExistingFolder_DoesNotRunRecursively()
    {
        // mock there's already a sidecar file in folder
        fileMock.Setup(x => x.Exists(SidecarFilePath)).Returns(true);
        
        SetupSubfolder();
        
        _ = await sut.Analyze(FolderName);
        
        // verify subfolders weren't even enumerated
        directoryMock.Verify(x => x.GetDirectories(FolderName), Times.Never);
        directoryMock.Verify(x => x.EnumerateDirectories(FolderName), Times.Never);
    }
    
    [Fact]
    public void Setup_NestedFolders_WritesCorrectValues()
    {
        // TODO
        //  implement a test that verifies that each level of nesting
        //  i.e. 0 and 1 gets its own unique values (so it's not duplicated / overwritten by a leaked state)
        //  (see behavior before aff3fe8(
    }

    private void SetupSubfolder()
    {
        // mock that subfolder exists
        directoryMock.Setup(x => x.Exists(SubFolderName)).Returns(true);
        directoryMock.Setup(x => x.Exists(SubFolderPath)).Returns(true);
        
        // mock that subfolder is listed
        directoryMock.Setup(x => x.GetDirectories(FolderName)).Returns(new[] {SubFolderName});
        directoryMock.Setup(x => x.GetDirectories(SubFolderPath)).Returns(Array.Empty<string>());
        directoryMock.Setup(x => x.EnumerateDirectories(FolderName)).Returns(new[] { SubFolderName });
        directoryMock.Setup(x => x.EnumerateDirectories(SubFolderPath)).Returns(new[] {SubFolderPath});
    }

    private static string CombinePaths(string part1, string part2)
    {
        return $"filesystem://{part1}/{part2}";
    }
}