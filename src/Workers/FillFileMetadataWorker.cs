using Microsoft.Extensions.Logging;
using Services;
using Services.Configuration;
using Services.Models;

namespace Workers;

public class FillFileMetadataWorker : BaseQueueWorker<FileToFillMetadata>
{
    private readonly IRawVideoService _fileStorageService;
    private readonly IWebVideoService _webVideoService;
    private readonly string _queueName;
    public FillFileMetadataWorker(
        ILogger<FillFileMetadataWorker> logger,
        IQueueService queueService,
        IRawVideoService fileStorageService,
        QueuesConfiguration queuesConfiguration,
        IWebVideoService webVideoService
    ) : base(logger, queueService)
    {
        _fileStorageService = fileStorageService;
        _queueName = queuesConfiguration.FillMetadataQueueName;
        _webVideoService = webVideoService;
    }
    protected override string QueueUrl => _queueName;

    protected override async Task ProcessMessage(FileToFillMetadata fileToFillMetadata, CancellationToken cancellationToken)
    {
        if (fileToFillMetadata.FileType == FileType.RawVideo)
        {
            await _fileStorageService.FillFileMetadataAsync(fileToFillMetadata.Id, cancellationToken);
        }
        else if(fileToFillMetadata.FileType == FileType.WebVideo)
        {
            await _webVideoService.FillFileMetadataAsync(fileToFillMetadata.Id, cancellationToken);
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}
