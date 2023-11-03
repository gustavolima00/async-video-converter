using Repositories;
using Repositories.Models;

namespace Services;

public class WebVideoServiceException : Exception
{
    public WebVideoServiceException(string message) : base(message) { }
}

public interface IWebVideoService
{
    Task FillFileMetadataAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WebVideo>> ListWebVideosAsync(CancellationToken cancellationToken = default);
}

public class WebVideoService : IWebVideoService
{
    private readonly IWebVideosRepository _webVideosRepository;
    private readonly IVideoManagerService _videoManagerService;

    public WebVideoService(IWebVideosRepository webVideosRepository, IVideoManagerService videoManagerService)
    {
        _webVideosRepository = webVideosRepository;
        _videoManagerService = videoManagerService;
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
}
