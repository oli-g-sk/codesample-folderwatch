using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ServerFolderWatch.Windows.Behaviors;

public static class ListBoxDoubleClick
{
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(ListBoxDoubleClick),
            new PropertyMetadata(null, OnCommandChanged));

    public static void SetCommand(DependencyObject element, ICommand? value)
    {
        element.SetValue(CommandProperty, value);
    }

    public static ICommand? GetCommand(DependencyObject element)
    {
        return (ICommand?)element.GetValue(CommandProperty);
    }

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListBox listBox)
            return;

        if (e.OldValue is not null)
            listBox.MouseDoubleClick -= OnMouseDoubleClick;

        if (e.NewValue is not null)
            listBox.MouseDoubleClick += OnMouseDoubleClick;
    }

    private static void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListBox listBox
            || e.OriginalSource is not DependencyObject source)
        {
            return;
        }

        var item = FindAncestor<ListBoxItem>(source);

        if (item is null)
            return;

        var command = GetCommand(listBox);
        var parameter = item.DataContext;

        if (command?.CanExecute(parameter) != true)
            return;

        command.Execute(parameter);
        e.Handled = true;
    }

    private static T? FindAncestor<T>(DependencyObject current)
        where T : DependencyObject
    {
        while (current is not T)
        {
            var parent = VisualTreeHelper.GetParent(current);

            if (parent is null)
                return null;

            current = parent;
        }

        return (T)current;
    }
}
