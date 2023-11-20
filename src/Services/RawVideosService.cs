using Clients.BlobStorage;
using Repositories;
using Repositories.Models;
using Services.Exceptions;

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
        Guid rawVideoUuid,
        CancellationToken cancellationToken = default
    );
    Task<RawVideo> GetAsync(
        int id,
        CancellationToken cancellationToken = default
    );
    Task UpdateSubtitleExtractionStatus(
        int id, AsyncTaskStatus status,
        CancellationToken cancellationToken = default
    );

    Task UpdateTrackExtractionStatus(
        int id, AsyncTaskStatus status,
        CancellationToken cancellationToken = default
    );

    Task<IEnumerable<RawVideo>> GetByUserUuidAsync(
        Guid userUuid, 
        CancellationToken cancellationToken = default
    );
}

public class RawVideosService : IRawVideoService
{
    private readonly IBlobStorageClient _blobStorageClient;
    private readonly IRawVideosRepository _rawVideosRepository;
    private readonly IQueueService _queueService;

    public RawVideosService(
        IBlobStorageClient blobStorageClient,
        IRawVideosRepository rawFilesRepository,
        IQueueService queueService
    )
    {
        _blobStorageClient = blobStorageClient;
        _rawVideosRepository = rawFilesRepository;
        _queueService = queueService;
    }

    public async Task<RawVideo> SaveAsync(Guid userUuid, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var folderPath = $"{userUuid}/raw_videos";
        var fileMetadata = await _blobStorageClient.UploadFileAsync(fileStream, fileName, folderPath, cancellationToken);
        var rawVideo = await _rawVideosRepository.CreateOrReplaceAsync(
            new RawVideo
            {
                Name = fileMetadata.Name,
                Path = fileMetadata.Path,
                UserUuid = userUuid
            }
            , cancellationToken);

        _queueService.EnqueueVideoToExtractTracks(new()
        {
            RawVideoId = rawVideo.Id,
            UserUuid = userUuid,
            RawVideoUuid = rawVideo.Uuid
        });
        _queueService.EnqueueVideoToExtractSubtitles(new()
        {
            RawVideoId = rawVideo.Id,
            UserUuid = userUuid,
            RawVideoUuid = rawVideo.Uuid
        });
        return rawVideo;
    }

    public async Task<RawVideo> GetAsync(Guid rawVideoUuid, CancellationToken cancellationToken = default)
    {
        return await _rawVideosRepository.GetByUuidAsync(rawVideoUuid, cancellationToken);
    }

    public async Task<IEnumerable<RawVideo>> GetByUserUuidAsync(Guid userUuid, CancellationToken cancellationToken = default)
    {
        return await _rawVideosRepository.GetByUserUuidAsync(userUuid, cancellationToken);
    }

    public async Task UpdateSubtitleExtractionStatus(int id, AsyncTaskStatus status, CancellationToken cancellationToken = default)
    {
        var rawFile = await _rawVideosRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with id {id} not found");
        rawFile.ExtractSubtitleStatus = status;
        await _rawVideosRepository.UpdateAsync(rawFile, cancellationToken);
    }

    public async Task<RawVideo> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var rawFile = await _rawVideosRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with id {id} not found");
        return rawFile;
    }

    public async Task UpdateTrackExtractionStatus(int id, AsyncTaskStatus status, CancellationToken cancellationToken = default)
    {
        var rawVideo = await _rawVideosRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with id {id} not found");
        rawVideo.ExtractTracksStatus = status;
        await _rawVideosRepository.UpdateAsync(rawVideo, cancellationToken);
    }
}
