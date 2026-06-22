using System.Diagnostics;
using System.IO.Abstractions;
using ServerFolderWatch.Desktop.Services;

namespace ServerFolderWatch.Windows.Services;

public sealed class ShellFileLauncher(IFileSystem fileSystem) : IFileLauncher
{
    public void Open(string filePath)
    {
        if (!fileSystem.File.Exists(filePath))
            return;

        Process.Start(new ProcessStartInfo(filePath)
        {
            UseShellExecute = true
        });
    }
}
