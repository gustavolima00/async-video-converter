using Repositories;
using Repositories.Models;

namespace Services;

public class WebVideoServiceException : Exception
{
    public WebVideoServiceException(string message) : base(message) { }
}

public interface IWebVideoService
{
    Task<IEnumerable<WebVideo>> ListWebVideosAsync(CancellationToken cancellationToken = default);
}

public class WebVideoService : IWebVideoService
{
    private readonly IWebVideosRepository _webVideosRepository;

    public WebVideoService(IWebVideosRepository webVideosRepository)
    {
        _webVideosRepository = webVideosRepository;
    }

    public async Task<IEnumerable<WebVideo>> ListWebVideosAsync(CancellationToken cancellationToken = default)
    {
        var webVideos = await _webVideosRepository.GetAllAsync(cancellationToken);
        return webVideos;
    }
}
