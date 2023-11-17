using Clients.BlobStorage;
using Repositories;
using Repositories.Models;
using Services.Exceptions;

namespace Services;

public interface IRawSubtitlesService
{
    Task<RawSubtitle> SaveAsync(
        Guid userUuid,
        Stream fileStream,
        string language,
        string rawVideoName,
        CancellationToken cancellationToken = default
    );

    Task<RawSubtitle> GetAsync(
        int id,
        CancellationToken cancellationToken = default
    );
}

public class RawSubtitlesService : IRawSubtitlesService
{
    private readonly IBlobStorageClient _blobStorageClient;
    private readonly IRawVideosRepository _rawFilesRepository;
    private readonly IMediaService _videoManagerService;
    private readonly IQueueService _queueService;

    public RawSubtitlesService(
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

    public async Task<RawSubtitle> SaveAsync(
        Guid userUuid,
        Stream fileStream,
        string language,
        string rawVideoName,
        CancellationToken cancellationToken = default
    )
    {
        var rawVideoPath = $"{userUuid}/raw_videos/{rawVideoName}";
        var rawVideo = await _rawFilesRepository.TryGetByPathAsync(rawVideoPath, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with path {rawVideoPath} not found");
        return await SaveAsync(fileStream, language, rawVideo, cancellationToken);
    }

    public async Task<RawSubtitle> SaveAsync(
        Stream fileStream,
        string language,
        RawVideo rawVideo,
        CancellationToken cancellationToken = default
    )
    {
        var folderPath = $"{rawVideo.UserUuid}/raw_subtitles";
        var fileName = $"{Path.GetFileNameWithoutExtension(rawVideo.Name)}_{language}.srt";
        var fileMetadata = await _blobStorageClient.UploadFileAsync(fileStream, fileName, folderPath, cancellationToken);
        var rawSubtitle = await _rawFilesRepository.CreateOrReplaceRawSubtitleAsync(
            new RawSubtitle
            {
                Path = fileMetadata.Path,
                Language = language ?? "und",
                RawVideoId = rawVideo.Id,
            }
            , cancellationToken);
        return rawSubtitle;
    }

    public async Task<RawSubtitle> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var rawSubtitle = await _rawFilesRepository.TryGetSubtitleByIdAsync(id, cancellationToken) ?? throw new RawRawSubtitlesServiceException($"Raw subtitle with id {id} not found");
        return rawSubtitle;
    }
}
