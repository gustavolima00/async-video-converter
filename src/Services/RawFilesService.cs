using Clients.BlobStorage;
using Repositories;
using Repositories.Models;
using Services.Models;

namespace Services;

public class RawVideoServiceException : Exception
{
    public RawVideoServiceException(string message) : base(message) { }
}

public interface IRawVideoService
{
    Task<RawVideo> SaveRawVideoAsync(Guid userUuid, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<RawVideo> FillFileMetadataAsync(int id, CancellationToken cancellationToken = default);
    Task<RawVideo> GetRawVideoAsync(Guid userUuid, string fileName, CancellationToken cancellationToken = default);
    Task<RawVideo> GetRawVideoAsync(int id, CancellationToken cancellationToken = default);
    Task<Stream> ConvertToMp4(int id, CancellationToken cancellationToken = default);
    Task UpdateConversionStatus(int id, ConversionStatus status, CancellationToken cancellationToken = default);
}

public class RawVideosService : IRawVideoService
{
    private readonly IBlobStorageClient _blobStorageClient;
    private readonly IRawVideosRepository _rawFilesRepository;
    private readonly IMediaService _videoManagerService;
    private readonly IQueueService _queueService;

    public RawVideosService(
        IBlobStorageClient blobStorageClient,
        IRawVideosRepository rawFilesRepository,
        IMediaService videoManagerService,
        IQueueService queueService
    )
    {
        _blobStorageClient = blobStorageClient;
        _rawFilesRepository = rawFilesRepository;
        _videoManagerService = videoManagerService;
        _queueService = queueService;
    }

    public async Task<RawVideo> SaveRawVideoAsync(Guid userUuid, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var folderPath = $"{userUuid}/raw_files";
        var fileMetadata = await _blobStorageClient.UploadFileAsync(fileStream, fileName, folderPath, cancellationToken);
        var rawFile = await _rawFilesRepository.CreateOrReplaceAsync(
            new RawVideo
            {
                Name = fileMetadata.Name,
                Path = fileMetadata.Path,
                UserUuid = userUuid
            }
            , cancellationToken);

        _queueService.EnqueueFileToFillMetadata(new()
        {
            Id = rawFile.Id,
            FileType = FileType.RawVideo
        });
        _queueService.EnqueueFileToConvert(rawFile.Id);
        return rawFile;
    }

    public async Task<RawVideo> GetRawVideoAsync(Guid userUuid, string fileName, CancellationToken cancellationToken = default)
    {
        string path = $"{userUuid}/raw_files/{fileName}";
        var rawFile = await _rawFilesRepository.TryGetByPathAsync(path, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with path {path} not found");
        return rawFile;
    }

    public async Task<RawVideo> FillFileMetadataAsync(int id, CancellationToken cancellationToken = default)
    {
        var rawFile = await _rawFilesRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with id {id} not found");
        var metadata = await _videoManagerService.GetFileMetadataAsync(rawFile.Path, cancellationToken);
        await _rawFilesRepository.UpdateMetadataAsync(id, metadata, cancellationToken);

        return rawFile;
    }

    public async Task UpdateConversionStatus(int id, ConversionStatus status, CancellationToken cancellationToken = default)
    {
        await _rawFilesRepository.UpdateConversionStatusAsync(id, status, cancellationToken);
    }

    public async Task<Stream> ConvertToMp4(int id, CancellationToken cancellationToken = default)
    {
        var rawFile = await _rawFilesRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with id {id} not found");
        var mp4Stream = await _videoManagerService.ConvertToMp4Async(rawFile.Path, cancellationToken);
        return mp4Stream;
    }

    public async Task<RawVideo> GetRawVideoAsync(int id, CancellationToken cancellationToken = default)
    {
        var rawFile = await _rawFilesRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with id {id} not found");
        return rawFile;
    }
}
