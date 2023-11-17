using Clients.BlobStorage;
using Repositories;
using Repositories.Models;
using Services.Exceptions;
using Services.Models;

namespace Services;

public interface IConvertedVideosService
{
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

    public async Task SaveConvertedVideoAsync(Stream stream, int rawFileId, CancellationToken cancellationToken = default)
    {
        await _convertedVideosRepository.CreateOrReplaceAsync(new()
        {
            RawVideoId = rawFileId
        }, cancellationToken);
    }
}
