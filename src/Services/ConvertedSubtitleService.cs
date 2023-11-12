using Clients.BlobStorage;
using Repositories;
using Services.Exceptions;
using Services.Models;

namespace Services;

public interface IConvertedSubtitleService
{
    Task FillSubtitleMetadataAsync(int id, CancellationToken cancellationToken = default);
    Task SaveConvertedSubtitleAsync(Stream stream, int rawFileId, CancellationToken cancellationToken = default);
}

public class ConvertedSubtitleService : IConvertedSubtitleService
{
    private readonly IConvertedVideosRepository _convertedVideosRepository;
    private readonly IMediaService _mediaService;
    private readonly IBlobStorageClient _blobStorageClient;
    private readonly IQueueService _queueService;
    private readonly IRawVideosRepository _rawVideosRepository;

    public ConvertedSubtitleService(
        IConvertedVideosRepository convertedVideosRepository,
        IMediaService videoManagerService,
        IQueueService queueService,
        IBlobStorageClient blobStorageClient,
        IRawVideosRepository rawFilesRepository
    )
    {
        _convertedVideosRepository = convertedVideosRepository;
        _mediaService = videoManagerService;
        _queueService = queueService;
        _blobStorageClient = blobStorageClient;
        _rawVideosRepository = rawFilesRepository;
    }

    public async Task FillSubtitleMetadataAsync(int id, CancellationToken cancellationToken = default)
    {
        var convertedSubtitles = await _convertedVideosRepository.TryGetSubtitleByIdAsync(id, cancellationToken) ?? throw new ConvertedSubtitleServiceException($"Raw file with id {id} not found");
        var metadata = await _mediaService.GetFileMetadataAsync(convertedSubtitles.Path, cancellationToken);
        await _convertedVideosRepository.UpdateSubtitleMetadataAsync(id, metadata, cancellationToken);
    }

    public async Task SaveConvertedSubtitleAsync(Stream stream, int rawSubtitleId, CancellationToken cancellationToken = default)
    {
        var rawSubtitle = await _rawVideosRepository.TryGetSubtitleByIdAsync(rawSubtitleId, cancellationToken) ?? throw new ConvertedSubtitleServiceException($"Raw subtitle with id {rawSubtitleId} not found");
        var rawVideo = await _rawVideosRepository.TryGetByIdAsync(rawSubtitle.RawVideoId, cancellationToken) ?? throw new ConvertedSubtitleServiceException($"Raw video with id {rawSubtitle.RawVideoId} not found");
        var convertedVideo = await _convertedVideosRepository.TryGetByRawVideoIdAsync(rawVideo.Id, cancellationToken) ?? throw new ConvertedSubtitleServiceException($"Converted video with raw video id {rawVideo.Id} not found");
        var folderPath = $"{rawSubtitle.UserUuid}/converted_subtitles";
        var fileName = $"{Path.GetFileNameWithoutExtension(rawSubtitle.Name)}.vtt";
        var fileMetadata = await _blobStorageClient.UploadFileAsync(stream, fileName, folderPath, cancellationToken);
        string subtitleLink = _blobStorageClient.GetLinkFromPath(fileMetadata.Path);
        var subtitle = await _convertedVideosRepository.CreateOrReplaceConvertedSubtitleAsync(new()
        {
            ConvertedVideoId = convertedVideo.Id,
            RawSubtitleId = rawSubtitleId,
            Link = subtitleLink,
            Path = fileMetadata.Path,
            Language = rawSubtitle.Language
        }, cancellationToken);

        _queueService.EnqueueFileToFillMetadata(new()
        {
            Id = subtitle.Id,
            FileType = FileType.ConvertedSubtitle
        });
    }
}
