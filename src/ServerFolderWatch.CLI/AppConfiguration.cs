using ServerFolderWatch.Core;

namespace ServerFolderWatch.CLI;

public class AppConfiguration : IAppConfiguration
{
    public string SidecarFileName { get; } = ".sidecar.json";

    public string RootPublicPath { get; } = "../../shared";
}