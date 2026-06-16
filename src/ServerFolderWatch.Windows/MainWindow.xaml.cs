using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ServerFolderWatch.Desktop.ViewModels;
using ServerFolderWatch.Desktop.ViewModels.Items;

namespace ServerFolderWatch.Windows;

public partial class MainWindow : Window
{
    private BrowseViewModel? viewModel;
    
    public MainWindow()
    {
        InitializeComponent();
        
        if (DataContext is BrowseViewModel viewModel)
            InitializeViewModel(viewModel);
        
        else
        {
            // TODO currently not needed
            DataContextChanged += OnDataContextChanged;
        }
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is BrowseViewModel viewModel)
        {
            InitializeViewModel(viewModel);            
            DataContextChanged -= OnDataContextChanged;
        }
    }

    private void InitializeViewModel(BrowseViewModel browseViewModel)
    {
        if (viewModel is not null)
            viewModel.FolderContents.PropertyChanged -= FolderContents_OnPropertyChanged;

        viewModel = browseViewModel;
        viewModel.FolderContents.PropertyChanged += FolderContents_OnPropertyChanged;
        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _ = browseViewModel.FolderTree.Initialize(folderPath);     
    }

    protected override void OnClosed(EventArgs e)
    {
        if (viewModel is not null)
            viewModel.FolderContents.PropertyChanged -= FolderContents_OnPropertyChanged;

        base.OnClosed(e);
    }

    private void FolderContents_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FolderContentsViewModel.IsRefreshing)
            && sender is FolderContentsViewModel { IsRefreshing: false })
        {
            FolderContentsListBox.Dispatcher.BeginInvoke(ScrollFolderContentsToTop,
                DispatcherPriority.ContextIdle);
        }
    }

    private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is FolderViewModel folder)
        {
            if (viewModel is not null)
                viewModel.FolderTree.SelectedFolder = folder;

            FolderTreeView.Dispatcher.BeginInvoke(ScrollSelectedTreeItemIntoView, DispatcherPriority.ContextIdle);
        }
    }

    private void ScrollSelectedTreeItemIntoView()
    {
        if (FolderTreeView.SelectedItem is null)
            return;

        var item = GetTreeViewItem(FolderTreeView, FolderTreeView.SelectedItem);
        item?.BringIntoView();
    }

    private static TreeViewItem? GetTreeViewItem(ItemsControl parent, object item)
    {
        parent.ApplyTemplate();
        parent.UpdateLayout();

        if (parent.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem directItem)
            return directItem;

        foreach (var child in parent.Items)
        {
            if (parent.ItemContainerGenerator.ContainerFromItem(child) is not TreeViewItem childItem)
                continue;

            var result = GetTreeViewItem(childItem, item);

            if (result is not null)
                return result;
        }

        return null;
    }

    private void ScrollFolderContentsToTop()
    {
        FolderContentsListBox.UpdateLayout();

        var scrollViewer = GetVisualChild<ScrollViewer>(FolderContentsListBox);
        scrollViewer?.ScrollToTop();
        scrollViewer?.ScrollToLeftEnd();
    }

    private static T? GetVisualChild<T>(DependencyObject parent)
        where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is T result)
                return result;

            var descendant = GetVisualChild<T>(child);

            if (descendant is not null)
                return descendant;
        }

        return null;
    }
}
