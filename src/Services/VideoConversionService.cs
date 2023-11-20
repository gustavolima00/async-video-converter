using Clients.BlobStorage;
using Repositories;
using Services.Exceptions;

namespace Services;

public interface IVideoConversionService
{
    Task ExtractVideoTracksAndConvertAsync(int rawVideoId, CancellationToken cancellationToken = default);
    Task ExtractSubtitlesAsync(int rawVideoId, CancellationToken cancellationToken = default);
    Task SaveSubtitleAsync(
        Guid userUuid,
        Stream fileStream,
        string fileExtension,
        string language,
        string rawVideoName,
        CancellationToken cancellationToken = default
    );
}

public class VideoConversionService : IVideoConversionService
{
    private readonly IConvertedVideosRepository _convertedVideosRepository;
    private readonly IConvertedVideoTracksRepository _convertedVideoTracksRepository;
    private readonly IMediaService _mediaService;
    private readonly IBlobStorageClient _blobStorageClient;
    private readonly IRawVideosRepository _rawVideosRepository;

    public VideoConversionService(
        IConvertedVideosRepository webVideosRepository,
        IMediaService videoManagerService,
        IBlobStorageClient blobStorageClient,
        IRawVideosRepository rawFilesRepository,
        IConvertedVideoTracksRepository convertedVideoTracksRepository
    )
    {
        _convertedVideosRepository = webVideosRepository;
        _mediaService = videoManagerService;
        _blobStorageClient = blobStorageClient;
        _rawVideosRepository = rawFilesRepository;
        _convertedVideoTracksRepository = convertedVideoTracksRepository;
    }

    private async Task SaveVideoTrackAsync(Stream stream, string language, int convertedVideoId, CancellationToken cancellationToken = default)
    {
        var convertedVideo = await _convertedVideosRepository.TryGetByIdAsync(convertedVideoId, cancellationToken) ?? throw new ConvertedVideoServiceException($"Converted video with id {convertedVideoId} not found");
        var rawVideo = await _rawVideosRepository.TryGetByIdAsync(convertedVideo.RawVideoId, cancellationToken) ?? throw new ConvertedVideoServiceException($"Raw video with id {convertedVideo.RawVideoId} not found");
        var folderPath = $"{rawVideo.UserUuid}/converted_videos";
        var fileName = $"{Path.GetFileNameWithoutExtension(rawVideo.Name)}_{language}.mp4";
        var fileMetadata = await _blobStorageClient.UploadFileAsync(stream, fileName, folderPath, cancellationToken);
        string videoLink = _blobStorageClient.GetLinkFromPath(fileMetadata.Path);
        await _convertedVideoTracksRepository.CreateOrReplaceAsync(new()
        {
            ConvertedVideoId = convertedVideo.Id,
            Link = videoLink,
            Path = fileMetadata.Path,
            Language = language ?? "und"
        }, cancellationToken);
    }

    private async Task SaveSubtitleTrackAsync(Stream stream, string language, int convertedVideoId, CancellationToken cancellationToken = default)
    {
        var convertedVideo = await _convertedVideosRepository.TryGetByIdAsync(convertedVideoId, cancellationToken) ?? throw new ConvertedVideoServiceException($"Converted video with id {convertedVideoId} not found");
        var rawVideo = await _rawVideosRepository.TryGetByIdAsync(convertedVideo.RawVideoId, cancellationToken) ?? throw new ConvertedVideoServiceException($"Raw video with id {convertedVideo.RawVideoId} not found");
        var folderPath = $"{rawVideo.UserUuid}/converted_videos";
        var fileName = $"{Path.GetFileNameWithoutExtension(rawVideo.Name)}_{language}.vtt";
        var fileMetadata = await _blobStorageClient.UploadFileAsync(stream, fileName, folderPath, cancellationToken);
        string subtitleLink = _blobStorageClient.GetLinkFromPath(fileMetadata.Path);
        await _convertedVideosRepository.CreateOrReplaceConvertedSubtitleAsync(new()
        {
            ConvertedVideoId = convertedVideo.Id,
            Link = subtitleLink,
            Path = fileMetadata.Path,
            Language = language ?? "und"
        }, cancellationToken);
    }

    public async Task ExtractVideoTracksAndConvertAsync(int rawVideoId, CancellationToken cancellationToken = default)
    {
        var rawVideo = await _rawVideosRepository.TryGetByIdAsync(rawVideoId, cancellationToken) ?? throw new ConvertedVideoServiceException($"Raw video with id {rawVideoId} not found");
        Console.WriteLine($"Extracting video tracks from {rawVideo.Path}, raw video id: {rawVideo.Id}");
        var convertedVideo = await _convertedVideosRepository.GetOrCreateByRawVideoIdAsync(rawVideo.Id, cancellationToken);
        var videoTracks = await _mediaService.ExtractVideoTracksAsync(rawVideo.Path, cancellationToken);
        var tasks = videoTracks.Select(videoTrackInfo => SaveVideoTrackAsync(videoTrackInfo.Stream, videoTrackInfo.Language, convertedVideo.Id, cancellationToken));
        await Task.WhenAll(tasks);
    }

    public async Task ExtractSubtitlesAsync(int rawVideoId, CancellationToken cancellationToken = default)
    {
        var rawVideo = await _rawVideosRepository.TryGetByIdAsync(rawVideoId, cancellationToken) ?? throw new RawRawSubtitlesServiceException($"Raw file with id {rawVideoId} not found");
        var convertedVideo = await _convertedVideosRepository.GetOrCreateByRawVideoIdAsync(rawVideo.Id, cancellationToken);
        var subtitles = await _mediaService.ExtractSubtitlesAsync(rawVideo.Path, cancellationToken);
        foreach (var subtitle in subtitles)
        {
            var vttStream = await _mediaService.ConvertSrtToVttAsync(subtitle.Stream, cancellationToken);
            await SaveSubtitleTrackAsync(vttStream, subtitle.Language, convertedVideo.Id, cancellationToken);
        }
    }

    private async Task<Stream> ConvertSubtitleAsync(
    Stream stream,
    string fileExtension,
    CancellationToken cancellationToken = default
)
    {
        if (fileExtension == ".vtt")
        {
            return stream;
        }
        if (fileExtension == ".srt")
        {
            return await _mediaService.ConvertSrtToVttAsync(stream, cancellationToken);
        }
        throw new RawRawSubtitlesServiceException($"Subtitle file extension {fileExtension} not supported");
    }

    public async Task SaveSubtitleAsync(
        Guid userUuid,
        Stream fileStream,
        string fileExtension,
        string language,
        string rawVideoName,
        CancellationToken cancellationToken = default
    )
    {
        var rawVideoPath = $"{userUuid}/raw_videos/{rawVideoName}";
        var subtitleStream = await ConvertSubtitleAsync(fileStream, fileExtension, cancellationToken);
        var rawVideo = await _rawVideosRepository.TryGetByPathAsync(rawVideoPath, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with path {rawVideoPath} not found");
        var convertedVideo = await _convertedVideosRepository.GetOrCreateByRawVideoIdAsync(rawVideo.Id, cancellationToken);
        await SaveSubtitleTrackAsync(subtitleStream, language, convertedVideo.Id, cancellationToken);
    }
}
