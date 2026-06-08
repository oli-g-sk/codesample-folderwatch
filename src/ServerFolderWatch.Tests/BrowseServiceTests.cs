using System.IO.Abstractions;
using Moq;
using ServerFolderWatch.Core;
using ServerFolderWatch.Core.Service;

namespace ServerFolderWatch.Tests;

public class BrowseServiceTests
{
    // TODO add tests for sorting of returned items
    
    private readonly Mock<IFileSystem> fileSystemMock = new();
    private readonly Mock<IConfiguration> configurationMock = new();
    
    private readonly BrowseService sut;
    
    public BrowseServiceTests()
    {
        sut = new BrowseService(configurationMock.Object, fileSystemMock.Object);
    }
    
    public void FromFolder_ExcludesSidecarFile()
    {
    }
}