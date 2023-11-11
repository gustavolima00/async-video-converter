using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services;
using Services.Configuration;
using Services.Models;

namespace Workers;

public class FillFileMetadataWorker : BaseQueueWorker<FileToFillMetadata>
{
    private readonly string _queueName;
    public FillFileMetadataWorker(
        ILogger<FillFileMetadataWorker> logger,
        IQueueService queueService,
        QueuesConfiguration queuesConfiguration,
        IServiceScopeFactory serviceScopeFactory
    ) : base(logger, queueService, serviceScopeFactory)
    {
        _queueName = queuesConfiguration.FillMetadataQueueName;
    }
    protected override string QueueUrl => _queueName;

    protected override Task ProcessMessage(IServiceScope scope, FileToFillMetadata fileToFillMetadata, CancellationToken cancellationToken)
    {
        var rawVideoService = scope.ServiceProvider.GetRequiredService<IRawVideoService>();
        var convertedVideosService = scope.ServiceProvider.GetRequiredService<IConvertedVideosService>();

        return fileToFillMetadata.FileType switch
        {
            FileType.RawVideo => rawVideoService.FillRawVideoMetadataAsync(fileToFillMetadata.Id, cancellationToken),
            FileType.RawSubtitle => rawVideoService.FillRawSubtitleMetadataAsync(fileToFillMetadata.Id, cancellationToken),
            FileType.ConvertedVideo => convertedVideosService.FillFileMetadataAsync(fileToFillMetadata.Id, cancellationToken),
            FileType.ConvertedSubtitle => convertedVideosService.FillSubtitleMetadataAsync(fileToFillMetadata.Id, cancellationToken),
            _ => throw new NotImplementedException(),
        };
    }
}
