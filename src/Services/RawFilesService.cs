﻿using Clients.BlobStorage;
using Clients.BlobStorage.Models;
using Repositories;
using Repositories.Models;
using Xabe.FFmpeg;

namespace Services;

public interface IRawFilesService
{
    Task<RawFile> SaveRawFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<RawFile> FillFileMetadataAsync(int id, CancellationToken cancellationToken = default);
    Task<RawFile> GetRawFileAsync(string path, CancellationToken cancellationToken = default);
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

    private async Task<RawFile> GetOrCreateFile(ObjectMetadata fileMetadata, CancellationToken cancellationToken = default)
    {
        var rawFile = await _rawFilesRepository.TryGetByPathAsync(fileMetadata.Path, cancellationToken);
        if (rawFile is null)
        {
            return await _rawFilesRepository.CreateAsync(new RawFile
            {
                Name = fileMetadata.Name,
                Path = fileMetadata.Path,
            }, cancellationToken);
        }
        return rawFile;
    }

    public async Task<RawFile> SaveRawFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var fileMetadata = await _blobStorageClient.UploadFileAsync(fileStream, fileName, "raw_files", cancellationToken);
        return await GetOrCreateFile(fileMetadata, cancellationToken);
    }

    public async Task<RawFile> GetRawFileAsync(string path, CancellationToken cancellationToken = default)
    {
        var rawFile = await _rawFilesRepository.TryGetByPathAsync(path, cancellationToken) ?? throw new Exception($"Raw file with path {path} not found");
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
        var rawFile = await _rawFilesRepository.TryGetByIdAsync(id, cancellationToken) ?? throw new Exception($"Raw file with id {id} not found");
        var metadata = await _videoManagerService.GetFileMetadata(rawFile.Path, cancellationToken);
        rawFile.Metadata = BuildFileMetadata(metadata);
        await _rawFilesRepository.UpdateAsync(rawFile, cancellationToken);

        return rawFile;
    }
}