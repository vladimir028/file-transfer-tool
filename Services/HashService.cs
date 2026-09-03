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
}