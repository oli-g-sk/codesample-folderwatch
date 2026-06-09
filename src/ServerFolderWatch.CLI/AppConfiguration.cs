using ServerFolderWatch.Core;

namespace ServerFolderWatch.CLI;

public class AppConfiguration : IAppConfiguration
{
    public string SidecarFileName => ".sidecar";
    
    public string RootPublicPath => ".";
}