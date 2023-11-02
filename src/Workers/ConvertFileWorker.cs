using Microsoft.Extensions.Logging;
using Services;
using Services.Configuration;

namespace Workers;

public class ConvertFileWorker : BaseQueueWorker<int>
{
    private readonly IRawFilesService _fileStorageService;
    readonly string _queueUrl;
    public ConvertFileWorker(
        ILogger<ConvertFileWorker> logger,
        IQueueService queueService,
        IRawFilesService fileStorageService,
        QueuesConfiguration queuesConfiguration
    ) : base(logger, queueService)
    {
        _fileStorageService = fileStorageService;
        _queueUrl = queuesConfiguration.ConvertQueueName;
    }
    protected override string QueueUrl => _queueUrl;
    protected override async Task ProcessMessage(int id, CancellationToken cancellationToken)
    {
        await _fileStorageService.ConvertFileToMp4(id, cancellationToken);
    }
}