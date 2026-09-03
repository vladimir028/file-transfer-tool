namespace FileTransferTool.Models;

public class ChunkInfo
{
    public int ChunkNumber { get; init; }
    public long Position { get; init; }
    public int Size { get; init; }
    public string Hash { get; init; } = string.Empty;
}