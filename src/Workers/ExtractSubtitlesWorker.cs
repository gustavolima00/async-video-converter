using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services;
using Services.Configuration;
using Services.Models;

namespace Workers;

public class ExtractSubtitlesWorker : BaseQueueWorker<VideoToExtractSubtitles>
{
    readonly string _queueUrl;
    public ExtractSubtitlesWorker(
        ILogger<ExtractSubtitlesWorker> logger,
        IQueueService queueService,
        QueuesConfiguration queuesConfiguration,
        IServiceScopeFactory serviceScopeFactory
    ) : base(logger, queueService, serviceScopeFactory)
    {
        _queueUrl = queuesConfiguration.WebhookQueueName;
    }
    protected override string QueueUrl => _queueUrl;
    protected override Task ProcessMessage(
        IServiceScope scope,
        VideoToExtractSubtitles videoToExtractSubtitles,
        CancellationToken cancellationToken
    )
    {
        var rawSubtitlesService = scope.ServiceProvider.GetRequiredService<IRawSubtitlesService>();
        return rawSubtitlesService.ExtractSubtitlesAsync(videoToExtractSubtitles.Id, cancellationToken);
    }
}
