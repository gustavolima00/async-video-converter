using BlobStorageSdk;
using BlobStorageSdk.Models;
using MediaToolkit.Model;
using Repositories;
using Repositories.Models;

namespace Services;

public class FileDetails
{
    public RawFile? RawFile { get; set; }
    public Metadata? Metadata { get; set; }
}

public interface IFileStorageService
{
    Task<FileDetails> SaveFileToConvertAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
}

public class FileStorageService : IFileStorageService
{
    private readonly IBlobStorageApi _blobStorageApi;
    private readonly IRawFilesRepository _rawFilesRepository;
    private readonly IVideoManagerService _videoManagerService;

    public FileStorageService(
        IBlobStorageApi blobStorageApi,
        IRawFilesRepository rawFilesRepository,
        IVideoManagerService videoManagerService
    )
    {
        _blobStorageApi = blobStorageApi;
        _rawFilesRepository = rawFilesRepository;
        _videoManagerService = videoManagerService;
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

    public async Task<FileDetails> SaveFileToConvertAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var fileMetadata = await _blobStorageApi.UploadFileAsync(fileStream, fileName, "raw_files", cancellationToken);
        var rawFile = await GetOrCreateFile(fileMetadata, cancellationToken);
        var metadata = await _videoManagerService.GetFileMetadata(fileMetadata.Path, cancellationToken);
        return new FileDetails
        {
            RawFile = rawFile,
            Metadata = metadata,
        };
    }
}