using ServerFolderWatch.Core;

namespace ServerFolderWatch.Server;

// TODO load from file
public class Configuration : IConfiguration
{
    public string SidecarFileName { get; } = ".sidecar.json";
    
    public string DefaultPath { get; } = "C:\\Temp";
}