using Clients.BlobStorage;
using Repositories;
using Repositories.Models;
using Services.Exceptions;

namespace Services;

public interface IConvertedVideosService
{
    Task<IEnumerable<ConvertedVideo>> ListConvertedVideosAsync(CancellationToken cancellationToken = default);
    Task ExtractVideoTracksAndConvertAsync(int rawVideoId, CancellationToken cancellationToken = default);
}

public class ConvertedVideosService : IConvertedVideosService
{
    private readonly IConvertedVideosRepository _convertedVideosRepository;
    private readonly IConvertedVideoTracksRepository _convertedVideoTracksRepository;
    private readonly IMediaService _mediaService;
    private readonly IBlobStorageClient _blobStorageClient;
    private readonly IRawVideosRepository _rawVideosRepository;

    public ConvertedVideosService(
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

    public async Task<IEnumerable<ConvertedVideo>> ListConvertedVideosAsync(CancellationToken cancellationToken = default)
    {
        var webVideos = await _convertedVideosRepository.GetAllAsync(cancellationToken);
        return webVideos;
    }

    public async Task SaveVideoTrackAsync(Stream stream, string language, int convertedVideoId, CancellationToken cancellationToken = default)
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
            Language = language
        }, cancellationToken);
    }

    public async Task ExtractVideoTracksAndConvertAsync(int rawVideoId, CancellationToken cancellationToken = default)
    {
        var rawVideo = await _rawVideosRepository.TryGetByIdAsync(rawVideoId, cancellationToken) ?? throw new ConvertedVideoServiceException($"Raw video with id {rawVideoId} not found");
        var convertedVideo = await _convertedVideosRepository.GetOrCreateByRawVideoIdAsync(rawVideo.Id, cancellationToken);
        var videoTracks = await _mediaService.ExtractVideoTracksAsync(rawVideo.Path, cancellationToken);
        var videoExtension = Path.GetExtension(rawVideo.Name);
        foreach (var videoTrackInfo in videoTracks)
        {
            var mp4Stream = await _mediaService.ConvertToMp4Async(videoTrackInfo.Stream, videoExtension, cancellationToken);
            await SaveVideoTrackAsync(mp4Stream, videoTrackInfo.Language, convertedVideo.Id, cancellationToken);
        }
    }
}
