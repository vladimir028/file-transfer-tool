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

    public void TransferFile(string sourcePath, string destinationPath)
    {
        byte[] buffer = new byte[_configuration.BufferSize];
        long totalBytes = new FileInfo(sourcePath).Length;

        long copiedBytes = 0;
        long position = 0;
        int chunkNumber = 0;

        using FileStream source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
        using FileStream destination = new FileStream(destinationPath, FileMode.Create, FileAccess.ReadWrite);

        int bytesRead;
        while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
        {
            chunkNumber++;
            string sourceHash = _hashService.CalculateMD5(buffer, bytesRead);

            bool verified = TransferAndVerifyChunk(destination, buffer, bytesRead, position, sourceHash, chunkNumber);

            if (!verified)
            {
                throw new IOException($"Chunk {chunkNumber} could not be verified.");
            }
            copiedBytes += bytesRead;
            _progressService.DisplayChunkInfo(new ChunkInfo
            {
                ChunkNumber = chunkNumber,
                Position = position,
                Size = bytesRead,
                Hash = sourceHash
            });

            var progress = new TransferProgress
            {
                ChunkNumber = chunkNumber,
                ChunkSizeMB = (double)bytesRead / (1024 * 1024),
                CopiedMB = (double)copiedBytes / (1024 * 1024),
                TotalMB = (double)totalBytes / (1024 * 1024),
                ProgressPercentage = (double)copiedBytes / totalBytes * 100
            };

            _progressService.DisplayTransferInfo(progress);
            position += bytesRead;
        }

        _progressService.DisplayTransferCompleted(sourcePath, destinationPath);
    }
    private bool TransferAndVerifyChunk(FileStream destination, byte[] buffer, int bytesRead, long position, string sourceHash, int chunkNumber)
    {
        int retryCount = 0;

        while (retryCount < _configuration.MaxRetries)
        {
            retryCount++;
            destination.Position = position;
            destination.Write(buffer, 0, bytesRead);
            destination.Position = position;

            byte[] destinationBuffer = new byte[bytesRead];
            int destinationBytesRead = destination.Read(destinationBuffer, 0, bytesRead);
            string destinationHash = _hashService.CalculateMD5(destinationBuffer, destinationBytesRead);
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
}