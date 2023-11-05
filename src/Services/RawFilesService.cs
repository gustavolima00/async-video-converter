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
    Task<RawFile> SaveRawFileAsync(Guid userUuid, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<RawFile> FillFileMetadataAsync(int id, CancellationToken cancellationToken = default);
    Task<RawFile> GetRawFileAsync(Guid userUuid, string fileName, CancellationToken cancellationToken = default);
    Task<Stream> ConvertToMp4(int id, CancellationToken cancellationToken = default);
    Task UpdateConversionStatus(int id, ConversionStatus status, CancellationToken cancellationToken = default);
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

    public async Task<RawFile> SaveRawFileAsync(Guid userUuid, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var folderPath = $"{userUuid}/raw_files";
        var fileMetadata = await _blobStorageClient.UploadFileAsync(fileStream, fileName, folderPath, cancellationToken);
        var rawFile = await _rawFilesRepository.CreateOrReplaceAsync(
            new RawFile
            {
                Name = fileMetadata.Name,
                Path = fileMetadata.Path,
                UserUuid = userUuid
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

    public async Task<RawFile> GetRawFileAsync(Guid userUuid, string fileName, CancellationToken cancellationToken = default)
    {
        string path = $"{userUuid}/raw_files/{fileName}";
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

    public async Task UpdateConversionStatus(int id, ConversionStatus status, CancellationToken cancellationToken = default)
    {
        await _rawFilesRepository.UpdateConversionStatusAsync(id, status, cancellationToken);
    }

    public async Task<Stream> ConvertToMp4(int id, CancellationToken cancellationToken = default)
    {
        var rawFile = await _rawFilesRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new RawFileServiceException($"Raw file with id {id} not found");
        var mp4Stream = await _videoManagerService.ConvertRawFileToMp4(rawFile.Path, cancellationToken);
        return mp4Stream;
    }
}
