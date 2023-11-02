using Microsoft.Extensions.Logging;
using Services;
using Services.Models;

namespace Workers;

public class FillFileMetadataWorker : BaseQueueWorker<FillFileMetadataMessage>
{
    private readonly ILogger<BaseQueueWorker<FillFileMetadataMessage>> _logger;
    public FillFileMetadataWorker(ILogger<BaseQueueWorker<FillFileMetadataMessage>> logger, IQueueService queueService) : base(logger, queueService)
    {
        _logger = logger;
    }
    protected override string QueueUrl => "fill_file_metadata";
    protected override int DelayAfterNoMessage => 2;
    protected override Task ProcessMessage(FillFileMetadataMessage fileMetadata, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing message: {@result}", fileMetadata);
        return Task.CompletedTask;
    }
}