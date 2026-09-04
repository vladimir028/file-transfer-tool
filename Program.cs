using System.Diagnostics;
using FileTransferTool.Factory;
using FileTransferTool.Services;

namespace FileTransferTool;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Write("Enter source file path: ");
        string? sourcePath = Console.ReadLine();
        
        Console.Write("Enter destination file path: ");
        string? destinationFolder = Console.ReadLine();

        if (!File.Exists(sourcePath))
        {
            Console.WriteLine("Source file does not exist");
            return;
        }

        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
        }
        
        string fileName = Path.GetFileName(sourcePath);
        string destinationPath = Path.Combine(destinationFolder, fileName);

        if (string.Equals(Path.GetFullPath(sourcePath), Path.GetFullPath(destinationPath), StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Source and destination paths cannot be the same.");
            return;
        }

        FactoryBuilder factory = new FactoryBuilder();
        FileTransferService transferService = factory.CreateFileTransferService(); 
        Stopwatch stopwatch = Stopwatch.StartNew();
        await transferService.TransferFileAsync(sourcePath, destinationPath);
        stopwatch.Stop();
        TimeSpan ts = stopwatch.Elapsed;
        Console.WriteLine($"Time: {ts.ToString(@"mm\:ss\.ff")}");
    }
}