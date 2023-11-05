
using Clients.BlobStorage;
using Clients.BlobStorage.Models;
using Clients.FFmpeg;
using Repositories.Models;

namespace Services;

public interface IVideoManagerService
{
    Task<MediaMetadata> GetFileMetadata(string path, CancellationToken cancellationToken = default);
    Task<Stream> ConvertRawFileToMp4(string path, CancellationToken cancellationToken = default);
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


    public async Task<MediaMetadata> GetFileMetadata(string path, CancellationToken cancellationToken = default)
    {
        var fileStream = await _blobStorageClient.GetFileAsync(path, cancellationToken) ?? throw new Exception($"File not found: {path}");
        var extension = Path.GetExtension(path);
        var mediaInfo = await _ffmpegClient.GetFileMetadata(fileStream, extension, cancellationToken);
        return new MediaMetadata(mediaInfo);
    }

    public async Task<Stream> ConvertRawFileToMp4(string path, CancellationToken cancellationToken = default)
    {
        var fileStream = await _blobStorageClient.GetFileAsync(path, cancellationToken) ?? throw new Exception($"File not found: {path}");
        var fileExtension = Path.GetExtension(path);
        var mp4Stream = await _ffmpegClient.ConvertToMp4(fileStream, fileExtension, cancellationToken);
        return mp4Stream;
    }
}
