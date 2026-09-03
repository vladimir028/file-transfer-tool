using FileTransferTool.Models;
using FileTransferTool.Services;

namespace FileTransferTool.Factory;

public class FactoryBuilder
{
    private TransferConfiguration CreateConfiguration()
    {
        return new TransferConfiguration
        {
            BufferSize = 4 * 1024 * 1024,
            MaxRetries = 3
        };
    }

    private HashService CreateHashService()
    {
        return new HashService();
    }

    private ProgressService CreateProgressService()
    {
        return new ProgressService();
    }
    
    public FileTransferService CreateFileTransferService()
    {
        TransferConfiguration configuration = CreateConfiguration();
        HashService hashService = CreateHashService();
        ProgressService progressService = CreateProgressService();

        return new FileTransferService(
            configuration,
            hashService,
            progressService);
    }
}