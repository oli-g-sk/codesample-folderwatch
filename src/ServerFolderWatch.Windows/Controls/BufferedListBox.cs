using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ServerFolderWatch.Desktop;

namespace ServerFolderWatch.Windows.Controls;

public class BufferedListBox : ListBox
{
    public static new readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(BufferedListBox),
            new PropertyMetadata(null, OnItemsSourceChanged));

    private ObservableCollection<object?> visibleItems = [];
    private INotifyCollectionChanged? currentObservableSource;
    private INotifyCollectionReplacement? currentReplacementSource;
    private bool replacementInProgress;
    private bool sourceIsDispatcherCollection;

    public BufferedListBox()
    {
        SetValue(System.Windows.Controls.ItemsControl.ItemsSourceProperty, visibleItems);
    }

    public new IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((BufferedListBox)d).SetSource((IEnumerable?)e.NewValue);
    }

    private void SetSource(IEnumerable? source)
    {
        if (currentObservableSource is not null)
            currentObservableSource.CollectionChanged -= Source_OnCollectionChanged;

        if (currentReplacementSource is not null)
            currentReplacementSource.ReplacementStarting -= Source_OnReplacementStarting;

        replacementInProgress = false;
        sourceIsDispatcherCollection = IsDispatcherCollection(source);
        visibleItems = CopyItems(source);
        SetValue(System.Windows.Controls.ItemsControl.ItemsSourceProperty, visibleItems);

        currentObservableSource = source as INotifyCollectionChanged;

        if (currentObservableSource is not null)
            currentObservableSource.CollectionChanged += Source_OnCollectionChanged;

        currentReplacementSource = source as INotifyCollectionReplacement;

        if (currentReplacementSource is not null)
            currentReplacementSource.ReplacementStarting += Source_OnReplacementStarting;
    }

    private void Source_OnReplacementStarting(object? sender, EventArgs e)
    {
        BeginReplacement();
    }

    private void Source_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!sourceIsDispatcherCollection)
        {
            ApplyChange(e);
            return;
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                AddItems(e.NewItems, e.NewStartingIndex);
                break;

            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Reset:
                if (!replacementInProgress || ContainsVisibleItem(e.OldItems))
                    BeginReplacement();
                break;

            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
                ApplyChange(e);
                break;
        }
    }

    private void BeginReplacement()
    {
        replacementInProgress = true;
        var oldItems = visibleItems;
        visibleItems = [];
        SetValue(System.Windows.Controls.ItemsControl.ItemsSourceProperty, visibleItems);
        ClearDetachedItemsAsync(oldItems);
    }

    private void ApplyChange(NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                AddItems(e.NewItems, e.NewStartingIndex);
                break;

            case NotifyCollectionChangedAction.Remove:
                RemoveItems(e.OldItems, e.OldStartingIndex);
                break;

            case NotifyCollectionChangedAction.Replace:
                RemoveItems(e.OldItems, e.OldStartingIndex);
                AddItems(e.NewItems, e.NewStartingIndex);
                break;

            case NotifyCollectionChangedAction.Move:
                MoveItems(e.OldItems, e.OldStartingIndex, e.NewStartingIndex);
                break;

            case NotifyCollectionChangedAction.Reset:
                visibleItems.Clear();
                break;
        }
    }

    private void AddItems(IList? items, int startingIndex)
    {
        if (items is null)
            return;

        var insertIndex = startingIndex >= 0
            ? Math.Min(startingIndex, visibleItems.Count)
            : visibleItems.Count;

        foreach (var item in items)
            visibleItems.Insert(insertIndex++, item);
    }

    private void RemoveItems(IList? items, int startingIndex)
    {
        if (items is null)
            return;

        if (startingIndex >= 0)
        {
            for (var i = 0; i < items.Count && startingIndex < visibleItems.Count; i++)
                visibleItems.RemoveAt(startingIndex);

            return;
        }

        foreach (var item in items)
            visibleItems.Remove(item);
    }

    private void MoveItems(IList? items, int oldStartingIndex, int newStartingIndex)
    {
        if (items is null || oldStartingIndex < 0 || newStartingIndex < 0)
            return;

        for (var i = 0; i < items.Count; i++)
        {
            var item = visibleItems[oldStartingIndex];
            visibleItems.RemoveAt(oldStartingIndex);
            visibleItems.Insert(Math.Min(newStartingIndex + i, visibleItems.Count), item);
        }
    }

    private void ClearDetachedItemsAsync(ObservableCollection<object?> items)
    {
        Dispatcher.BeginInvoke(() =>
        {
            if (items.Count > 0)
            {
                items.RemoveAt(0);
                ClearDetachedItemsAsync(items);
            }
        }, DispatcherPriority.ApplicationIdle);
    }

    private bool ContainsVisibleItem(IList? items)
    {
        if (items is null)
            return true;

        foreach (var item in items)
        {
            if (visibleItems.Contains(item))
                return true;
        }

        return false;
    }

    private static ObservableCollection<object?> CopyItems(IEnumerable? source)
    {
        var items = new ObservableCollection<object?>();

        if (source is null)
            return items;

        foreach (var item in source)
            items.Add(item);

        return items;
    }

    private static bool IsDispatcherCollection(IEnumerable? source)
    {
        var type = source?.GetType();

        while (type is not null)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(DispatcherCollection<>))
                return true;

            type = type.BaseType;
        }

        return false;
    }
}
