using System.Windows;
using ServerFolderWatch.Desktop.ViewModels;
using ServerFolderWatch.Desktop.ViewModels.Items;

namespace ServerFolderWatch.Windows;

public partial class MainWindow : Window
{
    private BrowseViewModel viewModel;
    
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
        viewModel = browseViewModel;
        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _ = browseViewModel.FolderTree.Initialize(folderPath);     
    }

    private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is FolderViewModel folder)
            viewModel.FolderTree.SelectedFolder = folder;
    }
}
