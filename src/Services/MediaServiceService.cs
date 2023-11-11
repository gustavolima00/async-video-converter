
using Clients.BlobStorage;
using Clients.FFmpeg;
using Repositories.Models;

namespace Services;

public interface IMediaService
{
    Task<MediaMetadata> GetFileMetadataAsync(string path, CancellationToken cancellationToken = default);
    Task<Stream> ConvertToMp4Async(string path, CancellationToken cancellationToken = default);
    Task<Stream> ConvertSrtToVttAsync(string path, CancellationToken cancellationToken = default);
}

public class MediaService : IMediaService
{

    private readonly IBlobStorageClient _blobStorageClient;
    private readonly IFFmpegClient _ffmpegClient;
    public MediaService(IBlobStorageClient blobStorageClient, IFFmpegClient ffmpegClient)
    {
        _blobStorageClient = blobStorageClient;
        _ffmpegClient = ffmpegClient;
    }


    public async Task<MediaMetadata> GetFileMetadataAsync(string path, CancellationToken cancellationToken = default)
    {
        var fileStream = await _blobStorageClient.GetFileAsync(path, cancellationToken) ?? throw new Exception($"File not found: {path}");
        var extension = Path.GetExtension(path);
        var mediaInfo = await _ffmpegClient.GetFileMetadata(fileStream, extension, cancellationToken);
        return new MediaMetadata(mediaInfo);
    }

    public async Task<Stream> ConvertToMp4Async(string path, CancellationToken cancellationToken = default)
    {
        var fileStream = await _blobStorageClient.GetFileAsync(path, cancellationToken) ?? throw new Exception($"File not found: {path}");
        var fileExtension = Path.GetExtension(path);
        var mp4Stream = await _ffmpegClient.ConvertToMp4(fileStream, fileExtension, cancellationToken);
        return mp4Stream;
    }

    public async Task<Stream> ConvertSrtToVttAsync(string path, CancellationToken cancellationToken = default)
    {
        var fileStream = await _blobStorageClient.GetFileAsync(path, cancellationToken) ?? throw new Exception($"File not found: {path}");
        var vttStream = await _ffmpegClient.ConvertSrtToVtt(fileStream, cancellationToken);
        return vttStream;
    }
}
