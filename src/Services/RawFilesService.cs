using Clients.BlobStorage;
using Repositories;
using Repositories.Models;
using Services.Models;

namespace Services;

public class RawFileServiceException : Exception
{
    public RawFileServiceException(string message) : base(message) { }
}

public interface IRawFilesService
{
    Task<RawFile> SaveRawFileAsync(Stream fileStream, string fileName, Guid? userUuid, CancellationToken cancellationToken = default);
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
    private readonly IWebVideoService _webVideoService;

    public RawFilesService(
        IBlobStorageClient blobStorageClient,
        IRawFilesRepository rawFilesRepository,
        IVideoManagerService videoManagerService,
        IQueueService queueService,
        IWebVideoService webVideoService
    )
    {
        _blobStorageClient = blobStorageClient;
        _rawFilesRepository = rawFilesRepository;
        _videoManagerService = videoManagerService;
        _queueService = queueService;
        _webVideoService = webVideoService;
    }

    public async Task<RawFile> SaveRawFileAsync(Stream fileStream, string fileName, Guid? userUuid, CancellationToken cancellationToken = default)
    {
        userUuid ??= Guid.NewGuid();
        var fileMetadata = await _blobStorageClient.UploadFileAsync(fileStream, fileName, "raw_files", cancellationToken);
        var rawFile = await _rawFilesRepository.CreateOrReplaceAsync(
            new RawFile
            {
                Name = fileMetadata.Name,
                Path = fileMetadata.Path,
                UserUuid = userUuid.Value
            }
            , cancellationToken);

        _queueService.EnqueueFileToFillMetadata(new()
        {
            Id = rawFile.Id,
            FileType = FileType.RawFile
        });
        _queueService.EnqueueFileToConvert(rawFile.Id);
        return rawFile;
    }

    public async Task<RawFile> GetRawFileAsync(string fileName, CancellationToken cancellationToken = default)
    {
        string path = $"raw_files/{fileName}";
        var rawFile = await _rawFilesRepository.TryGetByPathAsync(path, cancellationToken) ?? throw new RawFileServiceException($"Raw file with path {path} not found");
        return rawFile;
    }

    public async Task<RawFile> FillFileMetadataAsync(int id, CancellationToken cancellationToken = default)
    {
        var rawFile = await _rawFilesRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new RawFileServiceException($"Raw file with id {id} not found");
        var metadata = await _videoManagerService.GetFileMetadata(rawFile.Path, cancellationToken);
        await _rawFilesRepository.UpdateMetadataAsync(id, metadata, cancellationToken);

        return rawFile;
    }

    public async Task ConvertFileToMp4(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _rawFilesRepository.UpdateConversionStatusAsync(id, ConversionStatus.Converting, cancellationToken);
            var rawFile = await _rawFilesRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new RawFileServiceException($"Raw file with id {id} not found");
            var webVideoDetails = await _videoManagerService.ConvertRawFileToMp4(rawFile.Name, cancellationToken);
            await _rawFilesRepository.UpdateConversionStatusAsync(id, ConversionStatus.Converted, cancellationToken);

            await _webVideoService.CreateOrReplaceWebVideoAsync(webVideoDetails.Path, rawFile.Id, cancellationToken);
        }
        catch
        {
            await _rawFilesRepository.UpdateConversionStatusAsync(id, ConversionStatus.Error, cancellationToken);
            throw;
        }
    }
}
