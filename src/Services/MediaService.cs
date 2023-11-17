
using Clients.BlobStorage;
using Clients.FFmpeg;
using Services.Models;

namespace Services;

public interface IMediaService
{
    Task<Stream> ConvertToMp4Async(Stream stream, string fileExtension, CancellationToken cancellationToken = default);
    Task<Stream> ConvertSrtToVttAsync(Stream stream, CancellationToken cancellationToken = default);
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

    public async Task<Stream> ConvertToMp4Async(Stream stream, string fileExtension, CancellationToken cancellationToken = default)
    {
        var mp4Stream = await _ffmpegClient.ConvertToMp4(stream, fileExtension, cancellationToken);
        return mp4Stream;
    }

    public async Task<Stream> ConvertSrtToVttAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var vttStream = await _ffmpegClient.ConvertSrtToVtt(stream, cancellationToken);
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
