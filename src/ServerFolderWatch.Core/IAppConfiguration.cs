namespace ServerFolderWatch.Core;

public interface IAppConfiguration
{
    string SidecarFileName { get; }
    
    string RootPublicPath { get; }
}