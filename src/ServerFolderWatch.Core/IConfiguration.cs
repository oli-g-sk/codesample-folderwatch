namespace ServerFolderWatch.Core;

public interface IConfiguration
{
    string SidecarFileName { get; }
    
    string RootPublicPath { get; }
}