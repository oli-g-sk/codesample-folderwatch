namespace ServerFolderWatch.Desktop.Services;

public interface IDispatcherService
{
    const int DefaultPriority = 5;
    
    const int BackgroundPriority = 4;
    
    /// <param name="priority">
    /// An integer between 1 and 10, where 10 is the highest priority.
    /// Use <see cref="DefaultPriority"/> for a reasonable default
    /// (processed with the same priority as input), or <see cref="BackgroundPriority"/>
    /// or lower to offload/delay the processing for heavier UI workloads,
    /// such as rendering a lot of items in an ItemsControl.
    /// TODO create a custom enum for this
    /// </param>
    /// <returns></returns>
    Task InvokeAsync(Action action, int priority);
}