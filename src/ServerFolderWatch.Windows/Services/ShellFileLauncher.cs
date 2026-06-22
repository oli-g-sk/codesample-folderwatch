using Olivercode.Dry.System;
using ServerFolderWatch.Desktop.Services;

namespace ServerFolderWatch.Windows.Services;

public sealed class ShellFileLauncher : IFileLauncher
{
    public void Open(string filePath)
    {
        ProcessHelper.TryOpenFile(filePath);
    }
}
