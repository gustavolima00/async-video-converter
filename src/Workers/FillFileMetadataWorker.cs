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
        var rawVideosService = scope.ServiceProvider.GetRequiredService<IRawVideoService>();
        var rawSubtitlesService = scope.ServiceProvider.GetRequiredService<IRawSubtitlesService>();
        var convertedVideosService = scope.ServiceProvider.GetRequiredService<IConvertedVideosService>();
        var convertedSubtitlesService = scope.ServiceProvider.GetRequiredService<IConvertedSubtitleService>();

        return fileToFillMetadata.FileType switch
        {
            FileType.RawVideo => rawVideosService.FillMetadataAsync(fileToFillMetadata.Id, cancellationToken),
            FileType.RawSubtitle => rawSubtitlesService.FillMetadataAsync(fileToFillMetadata.Id, cancellationToken),
            FileType.ConvertedVideo => convertedVideosService.FillFileMetadataAsync(fileToFillMetadata.Id, cancellationToken),
            FileType.ConvertedSubtitle => convertedSubtitlesService.FillSubtitleMetadataAsync(fileToFillMetadata.Id, cancellationToken),
            _ => throw new NotImplementedException(),
        };
    }
}
