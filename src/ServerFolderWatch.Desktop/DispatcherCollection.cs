using System.Collections.ObjectModel;
using ServerFolderWatch.Desktop.Services;

namespace ServerFolderWatch.Desktop;

// TODO move to a standalone library
// TODO allow adjusting of priority
// TODO allow setting "batch size" for add & clear operations
public class DispatcherCollection<T>(IDispatcherService dispatcherService) : ObservableCollection<T>
{
    private DateTime lastUpdate;

    public async Task AddRange(IEnumerable<T> newItems)
    {
        var currentUpdate = DateTime.Now;
        lastUpdate = currentUpdate;

        foreach (var item in newItems)
        {
            if (currentUpdate != lastUpdate)
                break;

            await dispatcherService.InvokeAsync(() =>
                Add(item), IDispatcherService.BackgroundPriority);
        }
    }

    public async Task ClearAsync()
    {
        var currentUpdate = DateTime.Now;
        lastUpdate = currentUpdate;

        foreach (var item in new List<T>(Items))
        {
            if (lastUpdate != currentUpdate)
            {
                Clear();
                break;
            }

            await dispatcherService.InvokeAsync(() =>
                Remove(item), IDispatcherService.BackgroundPriority);
        }
    }
}