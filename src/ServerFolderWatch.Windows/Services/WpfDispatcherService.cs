using System.Windows.Threading;
using ServerFolderWatch.Desktop.Services;

namespace ServerFolderWatch.Windows.Services;

public class WpfDispatcherService : IDispatcherService
{
    private readonly Dispatcher dispatcher = System.Windows.Application.Current.Dispatcher;

    public async Task InvokeAsync(Action action, DispatcherPriority priority)
    {
        await dispatcher.InvokeAsync(action, priority);
    }
}
