using Microsoft.Extensions.Logging;
using Services;

namespace Workers;

public class FillFileMetadataWorker : BaseQueueWorker<int>
{
    IRawFilesService _fileStorageService;
    public FillFileMetadataWorker(
        ILogger<BaseQueueWorker<int>> logger,
        IQueueService queueService,
        IRawFilesService fileStorageService
    ) : base(logger, queueService)
    {
        _fileStorageService = fileStorageService;
    }
    protected override string QueueUrl => "fill_file_metadata";

    protected override async Task ProcessMessage(int id, CancellationToken cancellationToken)
    {
        await _fileStorageService.FillFileMetadataAsync(id, cancellationToken);
    }
}