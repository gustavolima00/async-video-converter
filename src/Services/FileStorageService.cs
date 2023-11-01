using BlobStorageSdk;

namespace Services;

public interface IFileStorageService
{
    Task SaveFileToConvertAsync(Stream fileStream, string fileName);
}

public class FileStorageService : IFileStorageService
{
    private readonly IBlobStorageApi _blobStorageApi;

    public FileStorageService(IBlobStorageApi blobStorageApi)
    {
        _blobStorageApi = blobStorageApi;
    }

    public async Task SaveFileToConvertAsync(Stream fileStream, string fileName)
    {
        await _blobStorageApi.UploadFileAsync(fileStream, fileName, "raw_files");
    }
}