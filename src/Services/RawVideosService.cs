using Clients.BlobStorage;
using Repositories;
using Repositories.Models;
using Services.Exceptions;
using Services.Models;

namespace Services;

public interface IRawVideoService
{
    Task<RawVideo> SaveAsync(
        Guid userUuid,
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default
    );
    Task<RawVideo> GetAsync(
        Guid userUuid,
        string fileName,
        CancellationToken cancellationToken = default
    );
    Task<RawVideo> GetAsync(
        int id,
        CancellationToken cancellationToken = default
    );
    Task<Stream> ConvertToMp4Async(
        int id,
        CancellationToken cancellationToken = default
    );
    Task UpdateConversionStatusAsync(
        int id, ConversionStatus status,
        CancellationToken cancellationToken = default
    );
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

    public async Task<RawVideo> SaveAsync(Guid userUuid, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var folderPath = $"{userUuid}/raw_videos";
        var fileMetadata = await _blobStorageClient.UploadFileAsync(fileStream, fileName, folderPath, cancellationToken);
        var rawFile = await _rawFilesRepository.CreateOrReplaceAsync(
            new RawVideo
            {
                Name = fileMetadata.Name,
                Path = fileMetadata.Path,
                UserUuid = userUuid
            }
            , cancellationToken);

        _queueService.EnqueueFileToConvert(new()
        {
            Id = rawFile.Id,
            FileType = FileType.RawVideo
        });
        _queueService.EnqueueVideoToExtractSubtitles(new()
        {
            Id = rawFile.Id
        });
        return rawFile;
    }

    public async Task<RawVideo> GetAsync(Guid userUuid, string fileName, CancellationToken cancellationToken = default)
    {
        string path = $"{userUuid}/raw_videos/{fileName}";
        var rawFile = await _rawFilesRepository.TryGetByPathAsync(path, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with path {path} not found");
        return rawFile;
    }

    public async Task UpdateConversionStatusAsync(int id, ConversionStatus status, CancellationToken cancellationToken = default)
    {
        await _rawFilesRepository.UpdateConversionStatusAsync(id, status, cancellationToken);
    }

    public async Task<Stream> ConvertToMp4Async(int id, CancellationToken cancellationToken = default)
    {
        var rawFile = await _rawFilesRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with id {id} not found");
        var mp4Stream = await _videoManagerService.ConvertToMp4Async(rawFile.Path, cancellationToken);
        return mp4Stream;
    }

    public async Task<RawVideo> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var rawFile = await _rawFilesRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with id {id} not found");
        return rawFile;
    }
}
