using System.IO.Abstractions;
using Moq;
using ServerFolderWatch.Core;
using ServerFolderWatch.Core.Service;

namespace ServerFolderWatch.Tests;

public class BrowseServiceTests
{
    private readonly Mock<IFileSystem> fileSystemMock = new();
    private readonly Mock<IConfiguration> configurationMock = new();
    
    private readonly BrowseService sut;
    
    public BrowseServiceTests()
    {
        sut = new BrowseService(configurationMock.Object, fileSystemMock.Object);
    }
    
    [Fact]
    public void FromFolder_ExcludesSidecarFile()
    {
        // TODO
    }
}