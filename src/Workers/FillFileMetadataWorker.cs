using Microsoft.Extensions.Logging;
using Services;
using Services.Configuration;

namespace Workers;

public class FillFileMetadataWorker : BaseQueueWorker<int>
{
    private readonly IRawFilesService _fileStorageService;
    private readonly string _queueName;
    public FillFileMetadataWorker(
        ILogger<FillFileMetadataWorker> logger,
        IQueueService queueService,
        IRawFilesService fileStorageService,
        QueuesConfiguration queuesConfiguration
    ) : base(logger, queueService)
    {
        _fileStorageService = fileStorageService;
        _queueName = queuesConfiguration.FillMetadataQueueName;
    }
    protected override string QueueUrl => _queueName;

    protected override async Task ProcessMessage(int id, CancellationToken cancellationToken)
    {
        await _fileStorageService.FillFileMetadataAsync(id, cancellationToken);
    }
}