using ServerFolderWatch.Core;

namespace ServerFolderWatch.Server;

public class AppConfiguration() : IAppConfiguration
{
    public string SidecarFileName { get; set; }
    
    public string RootPublicPath { get; set; }
}