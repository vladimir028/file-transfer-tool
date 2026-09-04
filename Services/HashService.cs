using System.Security.Cryptography;

namespace FileTransferTool.Services;

public class HashService
{
    public string CalculateMD5(byte[] buffer, int bytesRead)
    {
        using MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(buffer, 0, bytesRead);
        return Convert.ToHexString(hash);
    }

    public async Task<string> CalculateSHA256Async(string filePath, int bufferSize)
    {
        await using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite,  bufferSize,  FileOptions.Asynchronous);
        using SHA256 sha256 = SHA256.Create();
        byte[] hash = await sha256.ComputeHashAsync(stream);
        return Convert.ToHexString(hash);
    }
}