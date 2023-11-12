using Clients.BlobStorage;
using Repositories;
using Repositories.Models;
using Services.Exceptions;
using Services.Models;

namespace Services;

public interface IConvertedVideosService
{
    Task FillFileMetadataAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConvertedVideo>> ListConvertedVideosAsync(CancellationToken cancellationToken = default);
    Task SaveConvertedVideoAsync(Stream stream, int rawFileId, CancellationToken cancellationToken = default);
}

public class ConvertedVideosService : IConvertedVideosService
{
    private readonly IConvertedVideosRepository _convertedVideosRepository;
    private readonly IMediaService _mediaService;
    private readonly IBlobStorageClient _blobStorageClient;
    private readonly IQueueService _queueService;
    private readonly IRawVideosRepository _rawVideosRepository;

    public ConvertedVideosService(
        IConvertedVideosRepository webVideosRepository,
        IMediaService videoManagerService,
        IQueueService queueService,
        IBlobStorageClient blobStorageClient,
        IRawVideosRepository rawFilesRepository
    )
    {
        _convertedVideosRepository = webVideosRepository;
        _mediaService = videoManagerService;
        _queueService = queueService;
        _blobStorageClient = blobStorageClient;
        _rawVideosRepository = rawFilesRepository;
    }

    public async Task<IEnumerable<ConvertedVideo>> ListConvertedVideosAsync(CancellationToken cancellationToken = default)
    {
        var webVideos = await _convertedVideosRepository.GetAllAsync(cancellationToken);
        return webVideos;
    }

    public async Task FillFileMetadataAsync(int id, CancellationToken cancellationToken = default)
    {
        var webVideo = await _convertedVideosRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with id {id} not found");
        var metadata = await _mediaService.GetFileMetadataAsync(webVideo.Path, cancellationToken);
        await _convertedVideosRepository.UpdateMetadataAsync(id, metadata, cancellationToken);
    }

    public async Task SaveConvertedVideoAsync(Stream stream, int rawFileId, CancellationToken cancellationToken = default)
    {
        var rawFile = await _rawVideosRepository.TryGetByIdAsync(rawFileId, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with id {rawFileId} not found");
        var folderPath = $"{rawFile.UserUuid}/converted_videos";
        var fileName = $"{Path.GetFileNameWithoutExtension(rawFile.Name)}.mp4";
        var fileMetadata = await _blobStorageClient.UploadFileAsync(stream, fileName, folderPath, cancellationToken);
        string webVideoLink = _blobStorageClient.GetLinkFromPath(fileMetadata.Path);
        var webVideo = await _convertedVideosRepository.CreateOrReplaceAsync(new()
        {
            Name = fileName,
            Link = webVideoLink,
            Path = fileMetadata.Path,
            RawVideoId = rawFileId
        }, cancellationToken);

        _queueService.EnqueueFileToFillMetadata(new()
        {
            Id = webVideo.Id,
            FileType = FileType.ConvertedVideo
        });
    }
}
