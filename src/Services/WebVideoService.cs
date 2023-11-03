using Clients.BlobStorage;
using Repositories;
using Repositories.Models;
using Services.Models;

namespace Services;

public class WebVideoServiceException : Exception
{
    public WebVideoServiceException(string message) : base(message) { }
}

public interface IWebVideoService
{
    Task FillFileMetadataAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WebVideo>> ListWebVideosAsync(CancellationToken cancellationToken = default);
    Task CreateOrReplaceWebVideoAsync(string path, int rawFileId, CancellationToken cancellationToken = default);
}

public class WebVideoService : IWebVideoService
{
    private readonly IWebVideosRepository _webVideosRepository;
    private readonly IVideoManagerService _videoManagerService;
    private readonly IBlobStorageClient _blobStorageClient;
    private readonly IQueueService _queueService;

    public WebVideoService(
        IWebVideosRepository webVideosRepository,
        IVideoManagerService videoManagerService,
        IQueueService queueService,
        IBlobStorageClient blobStorageClient
    )
    {
        _webVideosRepository = webVideosRepository;
        _videoManagerService = videoManagerService;
        _queueService = queueService;
        _blobStorageClient = blobStorageClient;
    }

    public async Task<IEnumerable<WebVideo>> ListWebVideosAsync(CancellationToken cancellationToken = default)
    {
        var webVideos = await _webVideosRepository.GetAllAsync(cancellationToken);
        return webVideos;
    }

    public async Task FillFileMetadataAsync(int id, CancellationToken cancellationToken = default)
    {
        var webVideo = await _webVideosRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new RawFileServiceException($"Raw file with id {id} not found");
        var metadata = await _videoManagerService.GetFileMetadata(webVideo.Path, cancellationToken);
        await _webVideosRepository.UpdateMetadataAsync(id, metadata, cancellationToken);
    }

    public async Task CreateOrReplaceWebVideoAsync(string path, int rawFileId, CancellationToken cancellationToken = default)
    {
        string webVideoLink = _blobStorageClient.GetLinkFromPath(path);
        string fileName = Path.GetFileName(path);
        var webVideo = await _webVideosRepository.CreateOrReplaceAsync(new()
        {
            Name = fileName,
            Link = webVideoLink,
            RawFileId = rawFileId
        }, cancellationToken);

        _queueService.EnqueueFileToFillMetadata(new()
        {
            Id = webVideo.Id,
            FileType = FileType.WebVideo
        });
    }
}
