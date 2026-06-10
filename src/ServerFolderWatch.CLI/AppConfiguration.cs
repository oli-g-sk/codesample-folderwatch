using ServerFolderWatch.Core;

namespace ServerFolderWatch.CLI;

public class AppConfiguration : IAppConfiguration
{
    public string SidecarFileName => ".sidecar";
    
    // TODO REMOVE, only relevant for ASP.NET (CLI tool works anywhere)
    public string RootPublicPath => Directory.GetCurrentDirectory();
}