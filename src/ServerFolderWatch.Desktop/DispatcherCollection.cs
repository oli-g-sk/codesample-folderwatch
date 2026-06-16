using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using ServerFolderWatch.Desktop.Services;

namespace ServerFolderWatch.Desktop;

// TODO move to a standalone library
// TODO allow adjusting of priority
// TODO allow setting "batch size" for add & clear operations
public class DispatcherCollection<T>(IDispatcherService dispatcherService) : ObservableCollection<T>
{
#if DEBUG
    // TODO remove
    private const bool SkipOptimizations = false;
#endif
    
    private DateTime lastUpdate;

    public async Task AddRange(IEnumerable<T> newItems)
    {
        var currentUpdate = DateTime.Now;
        lastUpdate = currentUpdate;

        foreach (var item in newItems)
        {
            if (currentUpdate != lastUpdate)
                break;
            
#if DEBUG
            if (SkipOptimizations)
            {
                Add(item);
                continue;
            }
#endif
            
            await dispatcherService.InvokeAsync(() =>
            {
                if (currentUpdate == lastUpdate)
                    Add(item);
            }, IDispatcherService.BackgroundPriority);
        }
    }

    public async Task ClearAsync()
    {
        var currentUpdate = DateTime.Now;
        lastUpdate = currentUpdate;
        
#if DEBUG
        if (SkipOptimizations)
        {
            Clear();
            return;
        }
#endif

        foreach (var item in new List<T>(Items))
        {
            if (lastUpdate != currentUpdate)
            {
                Clear();
                break;
            }

            await dispatcherService.InvokeAsync(() =>
            {
                if (currentUpdate == lastUpdate)
                    Remove(item);
            }, IDispatcherService.BackgroundPriority);
        }
    }
}