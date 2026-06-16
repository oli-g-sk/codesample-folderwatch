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

    public async Task ReplaceRangeAsync(IEnumerable<T> newItems, long updateVersion)
    {
        var materializedItems = newItems.ToList();
        var oldItemCount = await GetCountAsync(updateVersion);

        if (!IsCurrent(updateVersion))
            return;

        if (oldItemCount == 0)
        {
            await AddRangeAsync(materializedItems, updateVersion);
            return;
        }

        await RemoveFirstItemAsync(updateVersion);
        oldItemCount--;

        var clearTask = RemoveFirstItemsAsync(updateVersion, oldItemCount);
        await AddRangeAsync(materializedItems, updateVersion);
        await clearTask;
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

    private async Task<int> GetCountAsync(long updateVersion)
    {
        var count = 0;

        await dispatcherService.InvokeAsync(() =>
        {
            if (IsCurrent(updateVersion))
                count = Count;
        }, IDispatcherService.BackgroundPriority);

        return count;
    }

    private async Task RemoveFirstItemsAsync(long updateVersion, int count)
    {
        for (var i = 0; IsCurrent(updateVersion) && i < count; i++)
            await RemoveFirstItemAsync(updateVersion);
    }

    private async Task RemoveFirstItemAsync(long updateVersion)
    {
        await dispatcherService.InvokeAsync(() =>
        {
            if (IsCurrent(updateVersion) && Count > 0)
                RemoveAt(0);
        }, IDispatcherService.BackgroundPriority);
    }
}
