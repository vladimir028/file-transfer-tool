using FileTransferTool.Models;

namespace FileTransferTool.Services;

public class ProgressService
{
    public void DisplayTransferInfo(TransferProgress progress)
    {
        Console.WriteLine($"Chunk: {progress.ChunkNumber}");
        Console.WriteLine($"Chunk size: {progress.ChunkSizeMB:F2} MB");
        Console.WriteLine($"Copied: {progress.CopiedMB:F2} MB / {progress.TotalMB:F2} MB");
        Console.WriteLine($"Progress: {progress.ProgressPercentage:F2}%");
        Console.WriteLine();
    }
    
    public void DisplayChunkVerification(int chunkNumber, string sourceHash, string destinationHash)
    {
        Console.WriteLine($"Chunk {chunkNumber} verified successfully.");
        Console.WriteLine($"Source Hash: {sourceHash}");
        Console.WriteLine($"Destination Hash: {destinationHash}");
    }
    
    public void DisplayChunkFailure(int chunkNumber)
    {
        Console.WriteLine($"Chunk {chunkNumber} failed verification. Retrying...");
    }
    
    public void DisplayChunkInfo(ChunkInfo chunk)
    {
        Console.WriteLine($"{chunk.ChunkNumber}) " +
                          $"position = {chunk.Position}, " +
                          $"hash = {chunk.Hash}");
    }
    
    public void DisplayTransferCompleted(
        string sourcePath,
        string destinationPath)
    {
        Console.WriteLine("File transferred successfully!");
        Console.WriteLine($"From: {sourcePath}");
        Console.WriteLine($"To:   {destinationPath}");
    }

    public void DisplaySourceDestinationChecksums(string sourceChecksum, string destinationChecksum)
    {
        Console.WriteLine($"Source Checksum: {sourceChecksum}");
        Console.WriteLine($"Destination Checksum: {destinationChecksum}");
    }

    public void DisplayIncompleteDestinationDeleted(string destinationPath)
    {
        Console.WriteLine($"Deleted incomplete destination file: {destinationPath}");
    }

    public void DisplayIncompleteDestinationDeleteFailed(string destinationPath, string reason)
    {
        Console.WriteLine($"Could not delete the incomplete destination file: {destinationPath}");
        Console.WriteLine($"Reason: {reason}");
        Console.WriteLine("The file is incomplete and must not be used.");
    }
}