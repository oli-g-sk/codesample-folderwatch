using System.Globalization;
using System.Windows.Data;
using ServerFolderWatch.Desktop.ViewModels.Items;

namespace ServerFolderWatch.Windows.Converters;

public class FileSystemEntryToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is FolderViewModel folder)
            return folder.IsExpanded ? "📂" : "📁";

        if (value is FileViewModel)
            return "📄";

        return "❓";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}