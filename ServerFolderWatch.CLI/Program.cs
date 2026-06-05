using ServerFolderWatch.Core;

namespace ServerFolderWatch.CLI;

class Program
{
    static void Main(string[] args)
    {
        var fileSystemChangedService = new FileSystemChangedService();
        
        Console.WriteLine("Enter path (defaults to C:\\Temp):");
        
        var input = Console.ReadLine();
        var path = TryReadPath(input);

        if (path is null)
        {
            Console.WriteLine("Invalid path. Exiting...");
            return;
        }
        
        fileSystemChangedService.Setup(path);
    }

    static string? TryReadPath(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        if (!Path.Exists(input))
            return null;

        return input;
    }
}