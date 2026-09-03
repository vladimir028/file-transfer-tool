using FileTransferTool.Factory;
using FileTransferTool.Services;

namespace FileTransferTool;

class Program
{
    static void Main(string[] args)
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
        
        FactoryBuilder factory = new FactoryBuilder();
        FileTransferService transferService = factory.CreateFileTransferService();
        transferService.TransferFile(sourcePath, destinationPath);
    }
}