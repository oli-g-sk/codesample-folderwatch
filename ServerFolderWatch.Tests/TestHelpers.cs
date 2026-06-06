using System.IO.Abstractions;
using Moq;

namespace ServerFolderWatch.Tests;

internal static class TestHelpers
{
    private static readonly string Prefix = "filesystem://";
    
    internal static string GetPath(string folder)
    {
        return $"{Prefix}{folder}";
    }

    internal static string GetPath(string folder, string subfolder, Mock<IPath> pathMock)
    {
        string prefixed = GetPath(Path.Combine(folder, subfolder));
        pathMock.Setup(x => x.Combine(folder, subfolder)).Returns(prefixed);
        return prefixed;
    }
    
    internal static string GetPath(string folder, string subfolder, string file, Mock<IPath> pathMock)
    {
        string prefixed = GetPath(Path.Combine(folder, subfolder, file));
        pathMock.Setup(x => x.Combine(folder, subfolder)).Returns(prefixed);
        return prefixed;
    } 
}