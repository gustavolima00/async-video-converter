
using Clients.BlobStorage;
using Clients.FFmpeg;
using Xabe.FFmpeg;

namespace Services;

public interface IVideoManagerService
{
    Task<IMediaInfo> GetFileMetadata(Stream stream, CancellationToken cancellationToken = default);
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

    private async static Task<string> SaveStreamIntoTempFile(Stream stream, CancellationToken cancellationToken = default)
    {
        string tempFilePath = Path.GetTempFileName();
        Console.WriteLine($"Temp file path: {tempFilePath}");
        using var fileStream = File.Create(tempFilePath);
        Console.WriteLine($"File created: {tempFilePath}");
        stream.Seek(0, SeekOrigin.Begin);
        await stream.CopyToAsync(fileStream, cancellationToken);
        return tempFilePath;
    }

    public async Task<IMediaInfo> GetFileMetadata(Stream stream, CancellationToken cancellationToken = default)
    {
        string? videoFilePath = null;
        try
        {
            videoFilePath = await SaveStreamIntoTempFile(stream, cancellationToken);
            return await _ffmpegClient.GetFileMetadata(videoFilePath, cancellationToken);
        }
        finally
        {
            if (videoFilePath is not null)
            {
                File.Delete(videoFilePath);
            }
        }
    }

    public async Task<IMediaInfo> GetFileMetadata(string path, CancellationToken cancellationToken = default)
    {
        var fileStream = await _blobStorageClient.GetFileAsync(path, cancellationToken) ?? throw new Exception($"File not found: {path}");
        return await GetFileMetadata(fileStream, cancellationToken);
    }
}