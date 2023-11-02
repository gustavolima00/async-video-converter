using Clients.BlobStorage;
using Repositories;
using Repositories.Models;
using Xabe.FFmpeg;

namespace Services;

public class RawFileServiceException : Exception
{
    public RawFileServiceException(string message) : base(message) { }
}

public interface IRawFilesService
{
    Task<RawFile> SaveRawFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<RawFile> FillFileMetadataAsync(int id, CancellationToken cancellationToken = default);
    Task<RawFile> GetRawFileAsync(string path, CancellationToken cancellationToken = default);
    Task ConvertFileToMp4(int id, CancellationToken cancellationToken = default);
}

public class RawFilesService : IRawFilesService
{
    private readonly IBlobStorageClient _blobStorageClient;
    private readonly IRawFilesRepository _rawFilesRepository;
    private readonly IVideoManagerService _videoManagerService;
    private readonly IQueueService _queueService;

    public RawFilesService(
        IBlobStorageClient blobStorageClient,
        IRawFilesRepository rawFilesRepository,
        IVideoManagerService videoManagerService,
        IQueueService queueService
    )
    {
        _blobStorageClient = blobStorageClient;
        _rawFilesRepository = rawFilesRepository;
        _videoManagerService = videoManagerService;
        _queueService = queueService;
    }

    public async Task<RawFile> SaveRawFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var fileMetadata = await _blobStorageClient.UploadFileAsync(fileStream, fileName, "raw_files", cancellationToken);
        var rawFile = await _rawFilesRepository.CreateOrReplaceByPathAsync(fileMetadata.Name, fileMetadata.Path, cancellationToken);
        _queueService.EnqueueFileToFillMetadata(rawFile.Id);
        _queueService.EnqueueFileToConvert(rawFile.Id);
        return rawFile;
    }

    public async Task<RawFile> GetRawFileAsync(string fileName, CancellationToken cancellationToken = default)
    {
        string path = $"raw_files/{fileName}";
        var rawFile = await _rawFilesRepository.TryGetByPathAsync(path, cancellationToken) ?? throw new RawFileServiceException($"Raw file with path {path} not found");
        return rawFile;
    }

    private static Metadata BuildFileMetadata(IMediaInfo mediaInfo)
    {
        return new Metadata
        {
            Duration = mediaInfo.Duration,
            Size = mediaInfo.Size,
            AudioStreams = mediaInfo.AudioStreams.Select(
                (stream) => new Repositories.Models.AudioStream
                {
                    Duration = stream.Duration,
                    Bitrate = stream.Bitrate,
                    SampleRate = stream.SampleRate,
                    Channels = stream.Channels,
                    Language = stream.Language,
                    Title = stream.Title,
                    Default = stream.Default,
                    Forced = stream.Forced,
                }
            ),
            SubtitleStreams = mediaInfo.SubtitleStreams.Select(
                (stream) => new Repositories.Models.SubtitleStream
                {
                    Language = stream.Language,
                    Title = stream.Title,
                    Default = stream.Default,
                    Forced = stream.Forced,
                }
            ),
        };
    }

    public async Task<RawFile> FillFileMetadataAsync(int id, CancellationToken cancellationToken = default)
    {
        var rawFile = await _rawFilesRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new RawFileServiceException($"Raw file with id {id} not found");
        var metadata = await _videoManagerService.GetFileMetadata(rawFile.Path, cancellationToken);
        await _rawFilesRepository.UpdateMetadataAsync(id, BuildFileMetadata(metadata), cancellationToken);

        return rawFile;
    }

    public async Task ConvertFileToMp4(int id, CancellationToken cancellationToken = default)
    {
        var rawFile = await _rawFilesRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new RawFileServiceException($"Raw file with id {id} not found");
        var mp4FileMetadata = await _videoManagerService.ConvertRawFileToMp4(rawFile.Name, cancellationToken);
        rawFile.ConvertedPath = mp4FileMetadata.Path;
        await _rawFilesRepository.UpdateConvertedPathAsync(id, mp4FileMetadata.Path, cancellationToken);
    }
}