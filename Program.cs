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
        
        TransferFile(sourcePath, destinationPath);
        
        Console.WriteLine();
        
        Console.WriteLine("File transferred successfully!");
        Console.WriteLine($"From: {sourcePath}");
        Console.WriteLine($"To:   {destinationPath}");
    }
    
    static void TransferFile(string sourcePath, string destinationPath)
    {
        const int bufferSize = 4 * 1024 * 1024;
        byte[] buffer = new byte[bufferSize];
        long totalBytes = new FileInfo(sourcePath).Length;
        long copiedBytes = 0;

        using FileStream source = new FileStream(
            sourcePath,
            FileMode.Open,
            FileAccess.Read);

        using FileStream destination = new FileStream(
            destinationPath,
            FileMode.Create,
            FileAccess.Write);

        int bytesRead;

        while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
        {
            destination.Write(buffer, 0, bytesRead);

            copiedBytes += bytesRead;
            double progress = (double)copiedBytes / totalBytes * 100;
            Console.WriteLine($"Progress: {progress:F2}%");
        }
    }
}