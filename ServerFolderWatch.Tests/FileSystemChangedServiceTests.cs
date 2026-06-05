using System.IO.Abstractions;
using Moq;
using ServerFolderWatch.Core;

namespace ServerFolderWatch.Tests;

public class FileSystemChangedServiceTests
{
    private const string Folder = "foo";
    private const string SidecarFileName = "bar.txt";
    private static string SidecarFilePath => $"filesystem://{Folder}/{SidecarFileName}";
    
    private readonly Mock<IPath> pathMock = new();
    private readonly Mock<IDirectory> directoryMock = new();
    private readonly Mock<IFile> fileMock = new();
    private readonly Mock<IConfiguration> configurationMock = new();
    
    private FileSystemChangedService sut;
    
    public FileSystemChangedServiceTests()
    {
        sut = new FileSystemChangedService(pathMock.Object,
            directoryMock.Object, fileMock.Object, configurationMock.Object);
        
        configurationMock.SetupGet(x => x.SidecarFileName)
            .Returns(SidecarFileName);
        pathMock.Setup(x => x.Combine(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(SidecarFilePath);
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
        
        bool actual = await sut.Setup("foo");
        Assert.Equal(sidecarFileExists, actual);
    }
    
    [Fact]
    public void Setup_CreatesSidecarFile()
    {
        
    }

    [Fact]
    public async Task Setup_ReadsConfiguration()
    {
        _ = await sut.Setup("foo");
        configurationMock.VerifyGet(x => x.SidecarFileName, Times.AtLeastOnce);
    }
}