
using Clients.BlobStorage;
using Clients.FFmpeg;
using Services.Models;

namespace Services;

public interface IMediaService
{
    Task<Stream> ConvertToMp4Async(string path, CancellationToken cancellationToken = default);
    Task<Stream> ConvertSrtToVttAsync(string path, CancellationToken cancellationToken = default);
    Task<IEnumerable<SubtitleInfo>> ExtractSubtitlesAsync(string path, CancellationToken cancellationToken = default);
    Task<IEnumerable<VideoTrackInfo>> ExtractVideoTracksAsync(string path, CancellationToken cancellationToken = default);
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

    public async Task<IEnumerable<SubtitleInfo>> ExtractSubtitlesAsync(string path, CancellationToken cancellationToken = default)
    {
        var fileStream = await _blobStorageClient.GetFileAsync(path, cancellationToken) ?? throw new Exception($"File not found: {path}");
        var subtitles = await _ffmpegClient.ExtractSubtitles(fileStream, cancellationToken);
        return subtitles.Select(s => new SubtitleInfo(s.metadata, s.stream));
    }

    public async Task<IEnumerable<VideoTrackInfo>> ExtractVideoTracksAsync(string path, CancellationToken cancellationToken = default)
    {
        var fileStream = await _blobStorageClient.GetFileAsync(path, cancellationToken) ?? throw new Exception($"File not found: {path}");
        var videoTracks = await _ffmpegClient.ExtractVideoTracks(path, cancellationToken);
        return videoTracks.Select(v => new VideoTrackInfo(v.metadata, v.stream));
    }
}
