using System.Collections.ObjectModel;
using ServerFolderWatch.Desktop.Services;

namespace ServerFolderWatch.Desktop;

// TODO move to a standalone library
// TODO allow adjusting of priority
// TODO allow setting "batch size" for add & clear operations
public class DispatcherCollection<T>(IDispatcherService dispatcherService) : ObservableCollection<T>
{
    private long currentUpdateVersion;

    public long BeginUpdate()
    {
        return Interlocked.Increment(ref currentUpdateVersion);
    }

    public bool IsCurrent(long updateVersion)
    {
        return Volatile.Read(ref currentUpdateVersion) == updateVersion;
    }

    public async Task AddRangeAsync(IEnumerable<T> newItems, long updateVersion)
    {
        using var enumerator = newItems.GetEnumerator();

        while (IsCurrent(updateVersion) && enumerator.MoveNext())
        {
            var item = enumerator.Current;
            
            await dispatcherService.InvokeAsync(() =>
            {
                if (IsCurrent(updateVersion))
                    Add(item);
            }, IDispatcherService.BackgroundPriority);
        }
    }

    public async Task ClearAsync(long updateVersion)
    {
        while (IsCurrent(updateVersion))
        {
            var removedItem = false;

            await dispatcherService.InvokeAsync(() =>
            {
                if (!IsCurrent(updateVersion) || Count == 0)
                    return;

                RemoveAt(0);
                removedItem = true;
            }, IDispatcherService.BackgroundPriority);

            if (!removedItem)
                break;
        }
    }
}
