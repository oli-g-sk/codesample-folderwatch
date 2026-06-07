using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;
using ServerFolderWatch.Core.Model;
using ServerFolderWatch.Core.Service;
using ServerFolderWatch.Core.Service.Interfaces;

namespace ServerFolderWatch.Tests;

public class MainServiceTests
{
    private readonly Mock<IPath> pathMock = new();
    private readonly Mock<IDirectory> directoryMock = new();
    private readonly Mock<IFile> fileMock = new();

    private readonly Mock<IFolderSnapshotService> persistenceServiceMock;
    
    private readonly FolderDiffService sut;
    
    private string FolderPath => TestHelpers.GetPath("foo");
    private string SubFolderPath => TestHelpers.GetPath("foo", "bar", pathMock);

    public MainServiceTests()
    {
        var browseServiceMock = new Mock<IBrowseService>();
        browseServiceMock.Setup(x => x.ListContents(It.IsAny<string>()))
            .Returns(FolderSnapshot.Empty);
        
        persistenceServiceMock = new Mock<IFolderSnapshotService>();
        
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);
        
        var fileSystemMock = new Mock<IFileSystem>();
        fileSystemMock.SetupGet(x => x.Directory).Returns(directoryMock.Object);
        fileSystemMock.SetupGet(x => x.Path).Returns(pathMock.Object);
        fileSystemMock.SetupGet(x => x.File).Returns(fileMock.Object);

        sut = new FolderDiffService(fileSystemMock.Object, loggerFactoryMock.Object);
    }
    
    /*
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Setup_NewFolder_InitializesIfNeeded(bool folderAlreadyMonitored)
    {
        persistenceServiceMock.Setup(x => x.IsFolderAlreadyMonitored(FolderPath))
            .Returns(folderAlreadyMonitored);

        Times timesToCallInitialization = folderAlreadyMonitored ? Times.Never() : Times.Once();
        bool wasInitialized = await sut.Analyze(FolderPath);

        persistenceServiceMock.Verify(x => x.InitializeFolder(FolderPath),timesToCallInitialization);
        Assert.NotEqual(folderAlreadyMonitored, wasInitialized);
    }

    [Fact]
    public async Task Setup_NewFolder_EnumeratesSubfolders()
    {
        persistenceServiceMock.Setup(x => x.IsFolderAlreadyMonitored(FolderPath)).Returns(false);
        persistenceServiceMock.Setup(x => x.IsFolderAlreadyMonitored(SubFolderPath)).Returns(false);
        
        SetupSubfolder();
        
        _ = await sut.Analyze(FolderPath);
        
        bool directoriesListed = directoryMock.Invocations
            .Any(x => x.Method.Name == nameof(IDirectory.GetDirectories));
        bool directoriesEnumerated = directoryMock.Invocations
            .Any(x => x.Method.Name == nameof(IDirectory.EnumerateDirectories));
        
        Assert.True(directoriesListed || directoriesEnumerated);
    }

    [Fact]
    public async Task Setup_NewFolder_RunsRecursively_InitializesSubfolders()
    {
        persistenceServiceMock.Setup(x => x.IsFolderAlreadyMonitored(FolderPath)).Returns(false);
        persistenceServiceMock.Setup(x => x.IsFolderAlreadyMonitored(SubFolderPath)).Returns(false);
        
        SetupSubfolder();
        
        _ = await sut.Analyze(FolderPath);
        
        // verify the subfolder was initialized
        persistenceServiceMock.Verify(x => x.InitializeFolder(SubFolderPath), Times.Once);
    }

    [Fact]
    public async Task Setup_ExistingFolder_DoesNotRunRecursively()
    {
        persistenceServiceMock.Setup(x => x.IsFolderAlreadyMonitored(FolderPath)).Returns(true);
        persistenceServiceMock.Setup(x => x.IsFolderAlreadyMonitored(SubFolderPath)).Returns(true);
        
        SetupSubfolder();
        
        _ = await sut.Analyze(FolderPath);
        
        // verify subfolders weren't even enumerated
        directoryMock.Verify(x => x.GetDirectories(FolderPath), Times.Never);
        directoryMock.Verify(x => x.EnumerateDirectories(FolderPath), Times.Never);
    }
        
    [Fact]
    public async Task Setup_ExistingFolder_NoFolderIsInitialized()
    {
        persistenceServiceMock.Setup(x => x.IsFolderAlreadyMonitored(FolderPath)).Returns(true);
        persistenceServiceMock.Setup(x => x.IsFolderAlreadyMonitored(SubFolderPath)).Returns(true);
        
        SetupSubfolder();
        
        _ = await sut.Analyze(FolderPath);
        
        persistenceServiceMock.Verify(x => x.InitializeFolder(It.IsAny<string>()), Times.Never);
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
        directoryMock.Setup(x => x.Exists(SubFolderPath)).Returns(true);
        
        // mock that subfolder is listed
        directoryMock.Setup(x => x.GetDirectories(FolderPath)).Returns(new[] {SubFolderPath});
        directoryMock.Setup(x => x.GetDirectories(SubFolderPath)).Returns(Array.Empty<string>());
        directoryMock.Setup(x => x.EnumerateDirectories(FolderPath)).Returns(new[] { SubFolderPath });
        directoryMock.Setup(x => x.EnumerateDirectories(SubFolderPath)).Returns(new[] {SubFolderPath});
    }
    */
}