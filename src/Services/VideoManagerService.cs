
using Clients.BlobStorage;
using Clients.BlobStorage.Models;
using Clients.FFmpeg;
using Repositories.Models;

namespace Services;

public interface IVideoManagerService
{
    Task<MediaMetadata> GetFileMetadata(string path, CancellationToken cancellationToken = default);
    Task<ObjectMetadata> ConvertRawFileToMp4(string fileName, CancellationToken cancellationToken = default);
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

    public async Task<ObjectMetadata> ConvertRawFileToMp4(string fileName, CancellationToken cancellationToken = default)
    {
        string rawFilePath = $"raw_files/{fileName}";
        string rawFileExtension = Path.GetExtension(rawFilePath);
        string mp4FileName = $"{Path.GetFileNameWithoutExtension(rawFilePath)}.mp4";
        var fileStream = await _blobStorageClient.GetFileAsync(rawFilePath, cancellationToken) ?? throw new Exception($"File not found: {rawFilePath}");
        Stream mp4Stream;
        if (rawFileExtension == ".mp4")
        {
            mp4Stream = fileStream;
        }
        else
        {
            mp4Stream = await _ffmpegClient.ConvertToMp4(fileStream, rawFileExtension, cancellationToken);
        }
        return await _blobStorageClient.UploadFileAsync(mp4Stream, mp4FileName, "mp4_files", cancellationToken);
    }
}