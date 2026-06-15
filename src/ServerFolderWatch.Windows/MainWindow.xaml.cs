using System.Windows;
using ServerFolderWatch.Desktop.ViewModels;

namespace ServerFolderWatch.Windows;

public partial class MainWindow : Window
{
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

    private void InitializeViewModel(BrowseViewModel viewModel)
    {
        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _ = viewModel.FolderTree.Initialize(folderPath);        
    }
}
