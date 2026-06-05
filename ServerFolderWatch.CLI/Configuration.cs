using ServerFolderWatch.Core;

namespace ServerFolderWatch.CLI;

public class Configuration : IConfiguration
{
    public string SidecarFileNames { get; } = ".sidecar";
}