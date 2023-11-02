using Xabe.FFmpeg;

namespace Clients.FFmpeg;

public interface IFFmpegClient
{
    Task<IMediaInfo> GetFileMetadata(string path, CancellationToken cancellationToken = default);
}

public class FFmpegClient : IFFmpegClient
{

    public FFmpegClient(FFmpegClientConfiguration configuration)
    {
        Xabe.FFmpeg.FFmpeg.SetExecutablesPath(
            configuration.DirectoryWithFFmpegAndFFprobe,
            configuration.FFmpegExeutableName,
            configuration.FFprobeExeutableName);
    }

    public async Task<IMediaInfo> GetFileMetadata(string path, CancellationToken cancellationToken = default)
    {
        return await Xabe.FFmpeg.FFmpeg.GetMediaInfo(path, cancellationToken);
    }
}