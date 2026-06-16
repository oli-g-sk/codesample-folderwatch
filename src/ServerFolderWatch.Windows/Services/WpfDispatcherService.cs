using System.Windows.Threading;
using ServerFolderWatch.Desktop.Services;

namespace ServerFolderWatch.Windows.Services;

public class WpfDispatcherService : IDispatcherService
{
    public async Task InvokeAsync(Action action, int priority)
    {
        await Dispatcher.CurrentDispatcher.InvokeAsync(action, (DispatcherPriority)priority);
    }
}