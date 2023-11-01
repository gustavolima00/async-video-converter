using BlobStorageSdk;
using BlobStorageSdk.Models;
using Repositories;
using Repositories.Models;

namespace Services;

public interface IFileStorageService
{
    Task<RawFile> SaveFileToConvertAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
}

public class FileStorageService : IFileStorageService
{
    private readonly IBlobStorageApi _blobStorageApi;
    private readonly IRawFilesRepository _rawFilesRepository;

    public FileStorageService(IBlobStorageApi blobStorageApi, IRawFilesRepository rawFilesRepository)
    {
        _blobStorageApi = blobStorageApi;
        _rawFilesRepository = rawFilesRepository;
    }

    private async Task<RawFile> GetOrCreateFile(ObjectMetadata fileMetadata, CancellationToken cancellationToken = default)
    {
        var rawFile = await _rawFilesRepository.TryGetByPath(fileMetadata.Path, cancellationToken);
        if (rawFile is null)
        {
            return await _rawFilesRepository.Create(new RawFile
            {
                Name = fileMetadata.Name,
                Path = fileMetadata.Path,
            }, cancellationToken);
        }
        return rawFile;
    }

    public async Task<RawFile> SaveFileToConvertAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var fileMetadata = await _blobStorageApi.UploadFileAsync(fileStream, fileName, "raw_files", cancellationToken);
        return await GetOrCreateFile(fileMetadata, cancellationToken);
    }
}