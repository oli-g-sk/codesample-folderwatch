namespace ServerFolderWatch.Server.PageModels;

public class BreadcrumbViewModel(string name, string url, string icon, bool isMonitored)
{
    public string Name { get; } = name;

    public string Url { get; } = url;

    public string Icon { get; } = icon;

    public bool IsMonitored { get; } = isMonitored;
}
