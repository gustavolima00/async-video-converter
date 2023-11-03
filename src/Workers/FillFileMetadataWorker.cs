using Microsoft.Extensions.Logging;
using Services;
using Services.Configuration;
using Services.Models;

namespace Workers;

public class FillFileMetadataWorker : BaseQueueWorker<FileToFillMetadata>
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

    protected override async Task ProcessMessage(FileToFillMetadata fileToFillMetadata, CancellationToken cancellationToken)
    {
        if (fileToFillMetadata.FileType == FileType.RawFile)
        {
            await _fileStorageService.FillFileMetadataAsync(fileToFillMetadata.Id, cancellationToken);
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}
