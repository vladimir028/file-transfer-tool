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
}