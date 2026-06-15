using ServerFolderWatch.Core;

namespace ServerFolderWatch.Windows;

public sealed class AppConfiguration : IAppConfiguration
{
    public string SidecarFileName => ".sidecar.json";

    // TODO support multiple drives
    
    // TODO REMOVE, only relevant for ASP.NET (CLI tool works anywhere)
    public string RootPublicPath => @"C:\";
}
