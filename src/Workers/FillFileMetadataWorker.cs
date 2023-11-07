using Microsoft.Extensions.Logging;
using Services;
using Services.Configuration;
using Services.Models;

namespace Workers;

public class FillFileMetadataWorker : BaseQueueWorker<FileToFillMetadata>
{
    private readonly IRawVideoService _fileStorageService;
    private readonly IConvertedVideosService _webVideoService;
    private readonly string _queueName;
    public FillFileMetadataWorker(
        ILogger<FillFileMetadataWorker> logger,
        IQueueService queueService,
        IRawVideoService fileStorageService,
        QueuesConfiguration queuesConfiguration,
        IConvertedVideosService webVideoService
    ) : base(logger, queueService)
    {
        _fileStorageService = fileStorageService;
        _queueName = queuesConfiguration.FillMetadataQueueName;
        _webVideoService = webVideoService;
    }
    protected override string QueueUrl => _queueName;

    protected override Task ProcessMessage(FileToFillMetadata fileToFillMetadata, CancellationToken cancellationToken)
    {
        return fileToFillMetadata.FileType switch
        {
            FileType.RawVideo => _fileStorageService.FillFileRawVideoMetadataAsync(fileToFillMetadata.Id, cancellationToken),
            FileType.RawSubtitle => _fileStorageService.FillRawSubtitleMetadataAsync(fileToFillMetadata.Id, cancellationToken),
            FileType.ConvertedVideo => _webVideoService.FillFileMetadataAsync(fileToFillMetadata.Id, cancellationToken),
            _ => throw new NotImplementedException(),
        };
    }
}
