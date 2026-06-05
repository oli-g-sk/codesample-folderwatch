using System.IO.Abstractions;
using Moq;
using ServerFolderWatch.Core;

namespace ServerFolderWatch.Tests;

public class FileSystemChangedServiceTests
{
    private const string Folder = "foo";
    private const string SidecarFileName = "bar.txt";
    private static string SidecarFilePath => CombinePaths(Folder, SidecarFileName);
    
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
    public async Task Setup_ReadsConfiguration()
    {
        _ = await sut.Setup("foo");
        configurationMock.VerifyGet(x => x.SidecarFileName, Times.AtLeastOnce);
    }
    
    [Fact]
    public async Task Setup_CreatesSidecarFile()
    {
        fileMock.Setup(x => x.Exists(SidecarFilePath)).Returns(false);
        
        _ = await sut.Setup("foo");
        
        fileMock.Verify(x => x.Create(SidecarFilePath), Times.Once);
    }

    [Fact]
    public async Task Setup_RunsRecursively()
    {
        fileMock.Setup(x => x.Exists(SidecarFilePath)).Returns(false);
        
        const string subFolderName = "subFolder";
        var subFolderPath = CombinePaths(Folder, subFolderName);
        
        // mock that subfolder exists
        directoryMock.Setup(x => x.Exists(subFolderName)).Returns(true);
        directoryMock.Setup(x => x.Exists(subFolderPath)).Returns(true);
        
        // mock that subfolder is listed
        directoryMock.Setup(x => x.GetDirectories(Folder)).Returns(new[] {subFolderName});
        directoryMock.Setup(x => x.GetDirectories(subFolderPath)).Returns(Array.Empty<string>());
        directoryMock.Setup(x => x.EnumerateDirectories(Folder)).Returns(new[] { subFolderName });
        directoryMock.Setup(x => x.EnumerateDirectories(subFolderPath)).Returns(new[] {subFolderPath});
        
        // mock there's no sidecar file in subfolder
        var subSidecarFilePath = CombinePaths(subFolderPath, SidecarFileName);
        fileMock.Setup(x => x.Exists(subSidecarFilePath)).Returns(false);
        
        // add sub-sidecar file path to mock
        pathMock.Setup(x => x.Combine(subFolderName, SidecarFileName))
            .Returns(subSidecarFilePath);
        
        _ = await sut.Setup(Folder);
        
        // verify a sidecar file was created in subfolder
        fileMock.Verify(x => x.Create(subSidecarFilePath), Times.Once);
    }

    private static string CombinePaths(string part1, string part2)
    {
        return $"filesystem://{part1}/{part2}";
    }
}