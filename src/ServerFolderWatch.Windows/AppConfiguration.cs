using ServerFolderWatch.Core;

namespace ServerFolderWatch.Windows;

public sealed class AppConfiguration : IAppConfiguration
{
    public string SidecarFileName => ".sidecar.json";

    // TODO support multiple drives via a Scoped lifetime for BrowseService
    public string RootPublicPath => @"C:\";
}
