using Clients.BlobStorage;
using Repositories;
using Repositories.Models;

namespace Services;

public class WebVideoException : Exception
{
    public WebVideoException(string message) : base(message) { }
}

public interface IWebVideoService
{
}

public class WebVideoService : IWebVideoService
{
    private readonly IWebVideosRepository _webVideosRepository;

    public WebVideoService(IWebVideosRepository webVideosRepository)
    {
        _webVideosRepository = webVideosRepository;
    }

    public async Task<IEnumerable<WebVideo>> GetWebVideosAsync(CancellationToken cancellationToken = default)
    {
        var webVideos = await _webVideosRepository.GetAllAsync(cancellationToken);
        return webVideos;
    }
}