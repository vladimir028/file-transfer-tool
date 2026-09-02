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
        int chunkNumber = 0;

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
            chunkNumber++;
            destination.Write(buffer, 0, bytesRead);

            copiedBytes += bytesRead;
            
            var progress = new TransferProgress
            {
                ChunkNumber = chunkNumber,
                ChunkSizeMB = (double)bytesRead / (1024 * 1024),
                CopiedMB = (double)copiedBytes / (1024 * 1024),
                TotalMB = (double)totalBytes / (1024 * 1024),
                ProgressPercentage = (double)copiedBytes / totalBytes * 100
            };
            DisplayTransferInfo(progress);
        }
    }

    private static string CalculateMD5(byte[] buffer, int bytesRead)
    {
        using MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(buffer, 0, bytesRead);
        return Convert.ToHexString(hash);
    }

    static void DisplayTransferInfo(TransferProgress progress)
    {
        Console.WriteLine($"Chunk: {progress.ChunkNumber}");
        Console.WriteLine($"Chunk size: {progress.ChunkSizeMB:F2} MB");
        Console.WriteLine($"Copied: {progress.CopiedMB:F2} MB / {progress.TotalMB:F2} MB");
        Console.WriteLine($"Progress: {progress.ProgressPercentage:F2}%");
        Console.WriteLine();
    }
}