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
        string sidecarFile = "sidecar.txt";

        directoryMock.Setup(x => x.Exists(It.IsAny<string>()))
            .Returns(true);
        fileMock.Setup(x => x.Exists(sidecarFile))
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