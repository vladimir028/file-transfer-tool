using Microsoft.Win32.SafeHandles;
using FileTransferTool.Models;

namespace FileTransferTool.Services;

public class FileTransferService
{
    private readonly TransferConfiguration _configuration;
    private readonly HashService _hashService;
    private readonly ProgressService _progressService;

    public FileTransferService(TransferConfiguration configuration, HashService hashService, ProgressService progressService)
    {
        _configuration = configuration;
        _hashService = hashService;
        _progressService = progressService;
    }

    public async Task TransferFileAsync(string sourcePath, string destinationPath)
    {
        int chunkSize = _configuration.BufferSize;
        byte[] buffer = new byte[chunkSize];
        long totalBytes = new FileInfo(sourcePath).Length;
        int chunkCount = (int)((totalBytes + chunkSize - 1) / chunkSize);

        long copiedBytes = 0;

        using SafeFileHandle source = File.OpenHandle(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.Asynchronous);
        using SafeFileHandle destination = File.OpenHandle(destinationPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, FileOptions.Asynchronous);

        RandomAccess.SetLength(destination, totalBytes);

        for (int index = 0; index < chunkCount; index++)
        {
            int chunkNumber = index + 1;
            long position = (long)index * chunkSize;
            int size = (int)Math.Min(chunkSize, totalBytes - position);

            await FillBufferAsync(source, buffer, size, position);
            string sourceHash = _hashService.CalculateMD5(buffer, size);

            bool verified = await TransferAndVerifyChunkAsync(destination, buffer, size, position, sourceHash, chunkNumber);

            if (!verified)
            {
                throw new IOException($"Chunk {chunkNumber} could not be verified.");
            }
            copiedBytes += size;
            _progressService.DisplayChunkInfo(new ChunkInfo
            {
                ChunkNumber = chunkNumber,
                Position = position,
                Size = size,
                Hash = sourceHash
            });

            var progress = new TransferProgress
            {
                ChunkNumber = chunkNumber,
                ChunkSizeMB = (double)size / (1024 * 1024),
                CopiedMB = (double)copiedBytes / (1024 * 1024),
                TotalMB = (double)totalBytes / (1024 * 1024),
                ProgressPercentage = totalBytes == 0 ? 100 : (double)copiedBytes / totalBytes * 100
            };

            _progressService.DisplayTransferInfo(progress);
        }

        if (IsSHAVerified(sourcePath, destinationPath))
        {
            _progressService.DisplayTransferCompleted(sourcePath, destinationPath);
        }
        
    }

    private bool IsSHAVerified(string sourcePath, string destinationPath)
    {
        string sourceChecksum = _hashService.CalculateSHA256(sourcePath);
        string destinationChecksum = _hashService.CalculateSHA256(destinationPath);

        if (!sourceChecksum.Equals(destinationChecksum, StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException("Final file verification failed.");
        }
        _progressService.DisplaySourceDestinationChecksums(sourceChecksum, destinationChecksum);
        return true;
    }

    private async Task<bool> TransferAndVerifyChunkAsync(SafeFileHandle destination, byte[] buffer, int size, long position, string sourceHash, int chunkNumber)
    {
        int retryCount = 0;

        while (retryCount < _configuration.MaxRetries)
        {
            retryCount++;
            await RandomAccess.WriteAsync(destination, buffer.AsMemory(0, size), position);

            byte[] destinationBuffer = new byte[size];
            await FillBufferAsync(destination, destinationBuffer, size, position);
            string destinationHash = _hashService.CalculateMD5(destinationBuffer, size);
            bool verified = sourceHash.Equals(destinationHash, StringComparison.OrdinalIgnoreCase);

            if (verified)
            {
                _progressService.DisplayChunkVerification(chunkNumber, sourceHash, destinationHash);
                return true;
            }
            _progressService.DisplayChunkFailure(chunkNumber);
        }
        return false;
    }

    private async Task FillBufferAsync(SafeFileHandle handle, byte[] buffer, int count, long offset)
    {
        int totalRead = 0;
        while (totalRead < count)
        {
            int bytesRead = await RandomAccess.ReadAsync(handle, buffer.AsMemory(totalRead, count - totalRead), offset + totalRead);
            if (bytesRead == 0)
            {
                throw new EndOfStreamException($"File ended unexpectedly at offset {offset + totalRead}.");
            }
            totalRead += bytesRead;
        }
    }
}
