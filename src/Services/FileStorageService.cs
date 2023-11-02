using Clients.BlobStorage;
using Clients.BlobStorage.Models;
using Repositories;
using Repositories.Models;
using Xabe.FFmpeg;

namespace Services;

public class FileDetails
{
    public RawFile? RawFile { get; set; }
    public IMediaInfo? Metadata { get; set; }
}

public interface IFileStorageService
{
    Task<FileDetails> SaveFileToConvertAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
}

public class FileStorageService : IFileStorageService
{
    private readonly IBlobStorageClient _blobStorageClient;
    private readonly IRawFilesRepository _rawFilesRepository;
    private readonly IVideoManagerService _videoManagerService;

    public FileStorageService(
        IBlobStorageClient blobStorageClient,
        IRawFilesRepository rawFilesRepository,
        IVideoManagerService videoManagerService
    )
    {
        _blobStorageClient = blobStorageClient;
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
        var fileMetadata = await _blobStorageClient.UploadFileAsync(fileStream, fileName, "raw_files", cancellationToken);
        var rawFile = await GetOrCreateFile(fileMetadata, cancellationToken);
        var metadata = await _videoManagerService.GetFileMetadata(fileMetadata.Path, cancellationToken);
        return new FileDetails
        {
            RawFile = rawFile,
            Metadata = metadata,
        };
    }
}