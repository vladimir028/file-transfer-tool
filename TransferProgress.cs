namespace FileTransferTool;

public class TransferProgress
{
    public int ChunkNumber { get; set; }
    public double ChunkSizeMB { get; set; }
    public double CopiedMB { get; set; }
    public double TotalMB { get; set; }
    public double ProgressPercentage { get; set; }
}