using Clients.BlobStorage;
using Repositories;
using Repositories.Models;
using Services.Models;

namespace Services;

public class RawVideoServiceException : Exception
{
    public RawVideoServiceException(string message) : base(message) { }
}

public interface IRawVideoService
{
    // Raw Videos
    Task<RawVideo> SaveRawVideoAsync(
        Guid userUuid,
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default
    );
    Task FillRawVideoMetadataAsync(
        int id,
        CancellationToken cancellationToken = default
    );
    Task<RawVideo> GetRawVideoAsync(
        Guid userUuid,
        string fileName,
        CancellationToken cancellationToken = default
    );
    Task<RawVideo> GetRawVideoAsync(
        int id,
        CancellationToken cancellationToken = default
    );
    Task<Stream> ConvertRawVideoToMp4Async(
        int id,
        CancellationToken cancellationToken = default
    );
    Task UpdateRawVideoConversionStatus(
        int id, ConversionStatus status,
        CancellationToken cancellationToken = default
    );

    Task ExtractSubtitlesAsync(
        int id,
        CancellationToken cancellationToken = default
    );

    // Raw Subtitles
    Task<RawSubtitle> SaveRawSubtitleAsync(
        Guid userUuid,
        Stream fileStream,
        string fileName,
        string rawVideoName,
        CancellationToken cancellationToken = default
    );

    Task<RawSubtitle> GetRawSubtitleAsync(
        int id,
        CancellationToken cancellationToken = default
    );

    Task UpdateRawSubtitleConversionStatus(
        int id, ConversionStatus status,
        CancellationToken cancellationToken = default
    );

    Task FillRawSubtitleMetadataAsync(
        int id,
        CancellationToken cancellationToken = default
    );

    Task<Stream> ConvertRawSubtitleToVttAsync(
        int id,
        CancellationToken cancellationToken = default
    );
}

public class RawVideosService : IRawVideoService
{
    private readonly IBlobStorageClient _blobStorageClient;
    private readonly IRawVideosRepository _rawFilesRepository;
    private readonly IMediaService _videoManagerService;
    private readonly IQueueService _queueService;

    public RawVideosService(
        IBlobStorageClient blobStorageClient,
        IRawVideosRepository rawFilesRepository,
        IMediaService videoManagerService,
        IQueueService queueService
    )
    {
        _blobStorageClient = blobStorageClient;
        _rawFilesRepository = rawFilesRepository;
        _videoManagerService = videoManagerService;
        _queueService = queueService;
    }

    public async Task<RawVideo> SaveRawVideoAsync(Guid userUuid, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var folderPath = $"{userUuid}/raw_videos";
        var fileMetadata = await _blobStorageClient.UploadFileAsync(fileStream, fileName, folderPath, cancellationToken);
        var rawFile = await _rawFilesRepository.CreateOrReplaceAsync(
            new RawVideo
            {
                Name = fileMetadata.Name,
                Path = fileMetadata.Path,
                UserUuid = userUuid
            }
            , cancellationToken);

        _queueService.EnqueueFileToFillMetadata(new()
        {
            Id = rawFile.Id,
            FileType = FileType.RawVideo
        });
        _queueService.EnqueueFileToConvert(new()
        {
            Id = rawFile.Id,
            FileType = FileType.RawVideo
        });
        return rawFile;
    }

    public async Task<RawVideo> GetRawVideoAsync(Guid userUuid, string fileName, CancellationToken cancellationToken = default)
    {
        string path = $"{userUuid}/raw_videos/{fileName}";
        var rawFile = await _rawFilesRepository.TryGetByPathAsync(path, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with path {path} not found");
        return rawFile;
    }

    public async Task FillRawVideoMetadataAsync(int id, CancellationToken cancellationToken = default)
    {
        var rawFile = await _rawFilesRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with id {id} not found");
        var metadata = await _videoManagerService.GetFileMetadataAsync(rawFile.Path, cancellationToken);
        await _rawFilesRepository.UpdateMetadataAsync(id, metadata, cancellationToken);
    }

    public async Task UpdateRawVideoConversionStatus(int id, ConversionStatus status, CancellationToken cancellationToken = default)
    {
        await _rawFilesRepository.UpdateConversionStatusAsync(id, status, cancellationToken);
    }

    public async Task<Stream> ConvertRawVideoToMp4Async(int id, CancellationToken cancellationToken = default)
    {
        var rawFile = await _rawFilesRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with id {id} not found");
        var mp4Stream = await _videoManagerService.ConvertToMp4Async(rawFile.Path, cancellationToken);
        return mp4Stream;
    }

    public async Task<RawVideo> GetRawVideoAsync(int id, CancellationToken cancellationToken = default)
    {
        var rawFile = await _rawFilesRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with id {id} not found");
        return rawFile;
    }

    public async Task<RawSubtitle> SaveRawSubtitleAsync(
        Guid userUuid,
        Stream fileStream,
        string fileName,
        string rawVideoName,
        CancellationToken cancellationToken = default
    )
    {
        var rawVideoPath = $"{userUuid}/raw_videos/{rawVideoName}";
        var rawVideo = await _rawFilesRepository.TryGetByPathAsync(rawVideoPath, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with path {rawVideoPath} not found");
        return await SaveRawSubtitleAsync(fileStream, fileName, rawVideo, cancellationToken);
    }

    public async Task<RawSubtitle> SaveRawSubtitleAsync(
        Stream fileStream,
        string fileName,
        RawVideo rawVideo,
        CancellationToken cancellationToken = default
    )
    {
        var folderPath = $"{rawVideo.UserUuid}/raw_subtitles";
        var fileMetadata = await _blobStorageClient.UploadFileAsync(fileStream, fileName, folderPath, cancellationToken);
        var rawSubtitle = await _rawFilesRepository.CreateOrReplaceRawSubtitleAsync(
            new RawSubtitle
            {
                Name = fileMetadata.Name,
                Path = fileMetadata.Path,
                RawVideoId = rawVideo.Id,
                UserUuid = rawVideo.UserUuid
            }
            , cancellationToken);
        _queueService.EnqueueFileToFillMetadata(new()
        {
            Id = rawSubtitle.Id,
            FileType = FileType.RawSubtitle
        });
        _queueService.EnqueueFileToConvert(new()
        {
            Id = rawSubtitle.Id,
            FileType = FileType.RawSubtitle
        });
        return rawSubtitle;
    }

    public async Task FillRawSubtitleMetadataAsync(int id, CancellationToken cancellationToken = default)
    {
        var rawSubtitle = await _rawFilesRepository.TryGetSubtitleByIdAsync(id, cancellationToken) ?? throw new RawVideoServiceException($"Raw subtitle with id {id} not found");
        var metadata = await _videoManagerService.GetFileMetadataAsync(rawSubtitle.Path, cancellationToken);
        await _rawFilesRepository.UpdateSubtitleMetadataAsync(id, metadata, cancellationToken);
    }

    public async Task<Stream> ConvertRawSubtitleToVttAsync(int id, CancellationToken cancellationToken = default)
    {
        var rawSubtitle = await _rawFilesRepository.TryGetSubtitleByIdAsync(id, cancellationToken) ?? throw new RawVideoServiceException($"Raw subtitle with id {id} not found");
        var vttStream = await _videoManagerService.ConvertSrtToVttAsync(rawSubtitle.Path, cancellationToken);
        return vttStream;
    }

    public async Task<RawSubtitle> GetRawSubtitleAsync(int id, CancellationToken cancellationToken = default)
    {
        var rawSubtitle = await _rawFilesRepository.TryGetSubtitleByIdAsync(id, cancellationToken) ?? throw new RawVideoServiceException($"Raw subtitle with id {id} not found");
        return rawSubtitle;
    }

    public async Task UpdateRawSubtitleConversionStatus(int id, ConversionStatus status, CancellationToken cancellationToken = default)
    {
        await _rawFilesRepository.UpdateSubtitleConversionStatusAsync(id, status, cancellationToken);
    }

    public async Task ExtractSubtitlesAsync(int id, CancellationToken cancellationToken = default)
    {
        var rawVideo = await _rawFilesRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with id {id} not found");
        var userUuid = rawVideo.UserUuid;
        var subtitles = await _videoManagerService.ExtractSubtitlesAsync(rawVideo.Path, cancellationToken);
        var subtitleNamePrefix = Path.GetFileNameWithoutExtension(rawVideo.Name);
        var subtitlesTasks = subtitles.Select(s =>
            SaveRawSubtitleAsync(s.Stream, $"{subtitleNamePrefix}_${s.Language}", rawVideo, cancellationToken)
        );
        await Task.WhenAll(subtitlesTasks);
    }
}
