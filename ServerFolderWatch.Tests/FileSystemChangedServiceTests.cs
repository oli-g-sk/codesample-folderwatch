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
    public void IsSetup_ThrowsForInvalidPath()
    {
        const string invalidPath = "abc";
        directoryMock.Setup(x => x.Exists(invalidPath)).Returns(false);
        Assert.Throws<ArgumentException>(() => sut.IsSetup(invalidPath));
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsSetup_ReturnsCorrectValue(bool sidecarFileExists)
    {
        string sidecarFile = "sidecar.txt";

        directoryMock.Setup(x => x.Exists(It.IsAny<string>()))
            .Returns(true);
        fileMock.Setup(x => x.Exists(sidecarFile))
            .Returns(sidecarFileExists);
        
        bool actual = sut.IsSetup("foo");
        Assert.Equal(sidecarFileExists, actual);
    }
    
    [Fact]
    public void Setup_ThrowsForInvalidPath()
    {
        
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