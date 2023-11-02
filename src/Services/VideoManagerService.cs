
using Clients.BlobStorage;
using Clients.FFmpeg;
using Xabe.FFmpeg;

namespace Services;

public interface IVideoManagerService
{
    Task<IMediaInfo> GetFileMetadata(string path, CancellationToken cancellationToken = default);
}

public class VideoManagerService : IVideoManagerService
{

    private readonly IBlobStorageClient _blobStorageClient;
    private readonly IFFmpegClient _ffmpegClient;
    public VideoManagerService(IBlobStorageClient blobStorageClient, IFFmpegClient ffmpegClient)
    {
        _blobStorageClient = blobStorageClient;
        _ffmpegClient = ffmpegClient;
    }


    public async Task<IMediaInfo> GetFileMetadata(string path, CancellationToken cancellationToken = default)
    {
        var fileStream = await _blobStorageClient.GetFileAsync(path, cancellationToken) ?? throw new Exception($"File not found: {path}");
        var extension = Path.GetExtension(path);
        return await _ffmpegClient.GetFileMetadata(fileStream, extension, cancellationToken);
    }
}