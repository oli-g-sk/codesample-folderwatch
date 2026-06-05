using System.IO.Abstractions;
using Moq;
using ServerFolderWatch.Core;

namespace ServerFolderWatch.Tests;

public class FileSystemChangedServiceTests
{
    private readonly Mock<IPath> pathMock = new();
    private readonly Mock<IDirectory> directoryMock = new();
    private readonly Mock<IFile> fileMock = new();
    private readonly Mock<IConfiguration> configurationMock = new();
    
    private FileSystemChangedService sut;
    
    public FileSystemChangedServiceTests()
    {
        sut = new FileSystemChangedService(pathMock.Object,
            directoryMock.Object, fileMock.Object, configurationMock.Object);
    }

    [Fact]
    public async Task Setup_ThrowsForInvalidPath()
    {
        const string invalidPath = "abc";
        directoryMock.Setup(x => x.Exists(invalidPath)).Returns(false);
        await Assert.ThrowsAsync<ArgumentException>(() => sut.Setup(invalidPath));
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Setup_ReturnsCorrectValue(bool sidecarFileExists)
    {
        const string sidecarFile = "sidecar.txt";
        configurationMock.SetupGet(x => x.SidecarFileName)
            .Returns(sidecarFile);

        const string folder = "myFolder";
        directoryMock.Setup(x => x.Exists(It.IsAny<string>()))
            .Returns(true);

        const string combinedPath = $"filesystem://{folder}/{sidecarFile}";
        pathMock.Setup(x => x.Combine(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(combinedPath);

        fileMock.Setup(x => x.Exists(sidecarFile))
            .Returns(sidecarFileExists);
        fileMock.Setup(x => x.Exists(combinedPath))
            .Returns(sidecarFileExists);
        
        bool actual = await sut.Setup("foo");
        Assert.Equal(sidecarFileExists, actual);
    }
    
    [Fact]
    public void Setup_ThrowsIfAlreadySetup()
    {
        
    }
    
    [Fact]
    public void Setup_CreatesSidecarFile()
    {
    }

    [Fact]
    public void Setup_ReadsConfiguration()
    {
    }
}