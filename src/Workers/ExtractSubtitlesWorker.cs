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
        _queueUrl = queuesConfiguration.ExtractSubtitlesQueueName;
    }
    protected override string QueueUrl => _queueUrl;
    protected override async Task ProcessMessage(
        IServiceScope scope,
        VideoToExtractSubtitles videoToExtractSubtitles,
        CancellationToken cancellationToken
    )
    {
        var videoConversionService = scope.ServiceProvider.GetRequiredService<IVideoConversionService>();
        var webhookService = scope.ServiceProvider.GetRequiredService<IWebhookService>();
        try
        {
            await videoConversionService.ExtractSubtitlesAsync(videoToExtractSubtitles.RawVideoId, cancellationToken);
            await webhookService.SendWebhookAsync(new()
            {
                Event = WebhookEvent.SubtitleTracksExtracted,
                UserUuid = videoToExtractSubtitles.UserUuid
            }, cancellationToken);
        }
        catch (Exception e)
        {
            await webhookService.SendWebhookAsync(new()
            {
                Event = WebhookEvent.SubtitleTrackExtractionFailed,
                UserUuid = videoToExtractSubtitles.UserUuid,
                Error = e.Message
            }, cancellationToken);
            throw;
        }
    }
}
