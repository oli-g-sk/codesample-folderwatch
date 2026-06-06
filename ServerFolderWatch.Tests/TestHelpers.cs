using System.IO.Abstractions;
using Moq;

namespace ServerFolderWatch.Tests;

public class TestHelpers
{
    private static string Prefix = "filesystem://";
    
    internal static string GetPath(string folder)
    {
        return $"{Prefix}{folder}";
    }

    internal static string GetPath(string folder, string subfolder, Mock<IPath> pathMock)
    {
        string prefixed = GetPath(Path.Combine(folder, subfolder));
        pathMock.SetupGet(x => x.Combine(folder, subfolder)).Returns(prefixed);
        return prefixed;
    }
    
    internal static string GetPath(string folder, string subfolder, string file, Mock<IPath> pathMock)
    {
        string prefixed = GetPath(Path.Combine(folder, subfolder, file));
        pathMock.SetupGet(x => x.Combine(folder, subfolder)).Returns(prefixed);
        return prefixed;
    } 
}