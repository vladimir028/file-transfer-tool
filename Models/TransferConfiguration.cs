namespace FileTransferTool.Models;

public class TransferConfiguration
{
    public int BufferSize { get; init; }
    public int MaxRetries { get; init; }
}