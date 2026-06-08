using ServerFolderWatch.Core;

namespace ServerFolderWatch.Server;

// TODO load from file
public class AppConfiguration : IAppConfiguration
{
    public string SidecarFileName { get; } = ".sidecar.json";
    
    public string RootPublicPath { get; } = "C:\\Temp";
}