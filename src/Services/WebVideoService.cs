﻿using Clients.BlobStorage;
using Repositories;
using Repositories.Models;
using Services.Models;

namespace Services;

public class WebVideoServiceException : Exception
{
    public WebVideoServiceException(string message) : base(message) { }
}

public interface IWebVideoService
{
    Task FillFileMetadataAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WebVideo>> ListWebVideosAsync(CancellationToken cancellationToken = default);
    Task SaveWebVideoAsync(Stream stream, int rawFileId, CancellationToken cancellationToken = default);
}

public class WebVideoService : IWebVideoService
{
    private readonly IWebVideosRepository _webVideosRepository;
    private readonly IMediaService _videoManagerService;
    private readonly IBlobStorageClient _blobStorageClient;
    private readonly IQueueService _queueService;
    private readonly IRawVideosRepository _rawFilesRepository;

    public WebVideoService(
        IWebVideosRepository webVideosRepository,
        IMediaService videoManagerService,
        IQueueService queueService,
        IBlobStorageClient blobStorageClient,
        IRawVideosRepository rawFilesRepository
    )
    {
        _webVideosRepository = webVideosRepository;
        _videoManagerService = videoManagerService;
        _queueService = queueService;
        _blobStorageClient = blobStorageClient;
        _rawFilesRepository = rawFilesRepository;
    }

    public async Task<IEnumerable<WebVideo>> ListWebVideosAsync(CancellationToken cancellationToken = default)
    {
        var webVideos = await _webVideosRepository.GetAllAsync(cancellationToken);
        return webVideos;
    }

    public async Task FillFileMetadataAsync(int id, CancellationToken cancellationToken = default)
    {
        var webVideo = await _webVideosRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with id {id} not found");
        var metadata = await _videoManagerService.GetFileMetadataAsync(webVideo.Path, cancellationToken);
        await _webVideosRepository.UpdateMetadataAsync(id, metadata, cancellationToken);
    }

    public async Task SaveWebVideoAsync(Stream stream, int rawFileId, CancellationToken cancellationToken = default)
    {
        var rawFile = await _rawFilesRepository.TryGetByIdAsync(rawFileId, cancellationToken) ?? throw new RawVideoServiceException($"Raw file with id {rawFileId} not found");
        var folderPath = $"{rawFile.UserUuid}/web_videos";
        var fileName = $"{Path.GetFileNameWithoutExtension(rawFile.Name)}.mp4";
        var fileMetadata = await _blobStorageClient.UploadFileAsync(stream, fileName, folderPath, cancellationToken);
        string webVideoLink = _blobStorageClient.GetLinkFromPath(fileMetadata.Path);
        var webVideo = await _webVideosRepository.CreateOrReplaceAsync(new()
        {
            Name = fileName,
            Link = webVideoLink,
            Path = fileMetadata.Path,
            RawVideoId = rawFileId
        }, cancellationToken);

        _queueService.EnqueueFileToFillMetadata(new()
        {
            Id = webVideo.Id,
            FileType = FileType.WebVideo
        });
    }
}
