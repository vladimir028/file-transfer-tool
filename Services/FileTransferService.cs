using System.Buffers;
using Microsoft.Win32.SafeHandles;
using FileTransferTool.Models;

namespace FileTransferTool.Services;

public class FileTransferService
{
    private readonly TransferConfiguration _configuration;
    private readonly HashService _hashService;
    private readonly ProgressService _progressService;
    private readonly double _bytesPerMegabyte = 1024.0 *  1024.0;

    public FileTransferService(TransferConfiguration configuration, HashService hashService, ProgressService progressService)
    {
        _configuration = configuration;
        _hashService = hashService;
        _progressService = progressService;
    }

    public async Task TransferFileAsync(string sourcePath, string destinationPath)
    {
        int chunkSize = _configuration.BufferSize;
        long totalBytes = new FileInfo(sourcePath).Length;
        int chunkCount = (int)((totalBytes + chunkSize - 1) / chunkSize);

        long copiedBytes = 0;
        Task<string> sourceChecksumTask = _hashService.CalculateSHA256Async(sourcePath, _configuration.BufferSize);

        using SafeFileHandle source = File.OpenHandle(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.Asynchronous);
        using SafeFileHandle destination = File.OpenHandle(destinationPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, FileOptions.Asynchronous);

        RandomAccess.SetLength(destination, totalBytes);

        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = _configuration.MaxConcurrency };

        await Parallel.ForEachAsync(Enumerable.Range(0, chunkCount), parallelOptions, async (index, _) =>
        {
            int chunkNumber = index + 1;
            long position = (long)index * chunkSize;
            int size = (int)Math.Min(chunkSize, totalBytes - position);

            byte[] buffer = ArrayPool<byte>.Shared.Rent(size);
            try
            {
                await FillBufferAsync(source, buffer, size, position);
                string sourceHash = _hashService.CalculateMD5(buffer, size);

                bool verified = await TransferAndVerifyChunkAsync(destination, buffer, size, position, sourceHash, chunkNumber);

                if (!verified)
                {
                    throw new IOException($"Chunk {chunkNumber} could not be verified.");
                }

                _progressService.DisplayChunkInfo(new ChunkInfo
                {
                    ChunkNumber = chunkNumber,
                    Position = position,
                    Size = size,
                    Hash = sourceHash
                });
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            long copied = Interlocked.Add(ref copiedBytes, size);

            var progress = new TransferProgress
            {
                ChunkNumber = chunkNumber,
                ChunkSizeMB = size / _bytesPerMegabyte,
                CopiedMB = copied / _bytesPerMegabyte,
                TotalMB = totalBytes / _bytesPerMegabyte,
                ProgressPercentage = totalBytes == 0 ? 100 : (double)copied / totalBytes * 100
            };

            _progressService.DisplayTransferInfo(progress);
        });

        string sourceChecksum = await sourceChecksumTask;
        if (await IsSHAVerifiedAsync(sourceChecksum, destinationPath))
        {
            _progressService.DisplayTransferCompleted(sourcePath, destinationPath);
        }
        
    }

    private async Task<bool> IsSHAVerifiedAsync(string? sourceChecksum, string destinationPath )
    {
        string destinationChecksum = await _hashService.CalculateSHA256Async(destinationPath, _configuration.BufferSize);

        if (!sourceChecksum.Equals(destinationChecksum, StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException("Final file verification failed.");
        }
        _progressService.DisplaySourceDestinationChecksums(sourceChecksum, destinationChecksum);
        return true;
    }

    private async Task<bool> TransferAndVerifyChunkAsync(SafeFileHandle destination, byte[] buffer, int size, long position, string sourceHash, int chunkNumber)
    {
        byte[] destinationBuffer = ArrayPool<byte>.Shared.Rent(size);
        try
        {
            int retryCount = 0;

            while (retryCount < _configuration.MaxRetries)
            {
                retryCount++;
                await RandomAccess.WriteAsync(destination, buffer.AsMemory(0, size), position);

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
        finally
        {
            ArrayPool<byte>.Shared.Return(destinationBuffer);
        }
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
