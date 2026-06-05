using System.IO.Abstractions;
using Moq;
using ServerFolderWatch.Core;

namespace ServerFolderWatch.Tests;

public class FileSystemChangedServiceTests
{
    private readonly Mock<IPath> pathMock = new();
    private readonly Mock<IDirectory> directoryMock = new();
    private readonly Mock<IFile> fileMock = new();
    
    private FileSystemChangedService sut;
    
    public FileSystemChangedServiceTests()
    {
        sut = new FileSystemChangedService(pathMock.Object, directoryMock.Object, fileMock.Object);
    }
    
    [Fact]
    public void IsSetup_ReturnsCorrectValue()
    {
        
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
}