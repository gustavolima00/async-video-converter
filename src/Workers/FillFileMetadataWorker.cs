using Microsoft.Extensions.Logging;
using Services;
using Services.Models;

namespace Workers;

public class FillFileMetadataWorker : BaseQueueWorker<FillFileMetadataMessage>
{
    IRawFilesService _fileStorageService;
    public FillFileMetadataWorker(
        ILogger<BaseQueueWorker<FillFileMetadataMessage>> logger,
        IQueueService queueService,
        IRawFilesService fileStorageService
    ) : base(logger, queueService)
    {
        _fileStorageService = fileStorageService;
    }
    protected override string QueueUrl => "fill_file_metadata";
    protected override int DelayAfterNoMessage => 2;
    protected override async Task ProcessMessage(FillFileMetadataMessage fileMetadata, CancellationToken cancellationToken)
    {
        await _fileStorageService.FillFileMetadataAsync(fileMetadata.Id, cancellationToken);
    }
}