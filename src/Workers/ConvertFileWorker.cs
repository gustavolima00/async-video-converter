using Microsoft.Extensions.Logging;
using Services;

namespace Workers;

public class ConvertFileWorker : BaseQueueWorker<int>
{
    IRawFilesService _fileStorageService;
    public ConvertFileWorker(
        ILogger<ConvertFileWorker> logger,
        IQueueService queueService,
        IRawFilesService fileStorageService
    ) : base(logger, queueService)
    {
        _fileStorageService = fileStorageService;
    }
    protected override string QueueUrl => "convert";
    protected override async Task ProcessMessage(int id, CancellationToken cancellationToken)
    {
        await _fileStorageService.ConvertFileToMp4(id, cancellationToken);
    }
}