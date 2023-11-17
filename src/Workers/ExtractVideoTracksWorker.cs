using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services;
using Services.Configuration;
using Services.Models;

namespace Workers;

public class ExtractVideoTracksWorker : BaseQueueWorker<VideoToExtractVideoTracks>
{
    readonly string _queueUrl;
    public ExtractVideoTracksWorker(
        ILogger<ExtractVideoTracksWorker> logger,
        IQueueService queueService,
        QueuesConfiguration queuesConfiguration,
        IServiceScopeFactory serviceScopeFactory
    ) : base(logger, queueService, serviceScopeFactory)
    {
        _queueUrl = queuesConfiguration.ExtractVideoTracksQueueName;
    }
    protected override string QueueUrl => _queueUrl;
    protected override async Task ProcessMessage(
        IServiceScope scope,
        VideoToExtractVideoTracks data,
        CancellationToken cancellationToken
    )
    {
        var videoConversionService = scope.ServiceProvider.GetRequiredService<IVideoConversionService>();
        var webhookService = scope.ServiceProvider.GetRequiredService<IWebhookService>();
        try
        {
            await videoConversionService.ExtractVideoTracksAndConvertAsync(data.RawVideoId, cancellationToken);
            await webhookService.SendWebhookAsync(new()
            {
                Event = WebhookEvent.VideoTracksExtracted,
                UserUuid = data.UserUuid
            }, cancellationToken);
        }
        catch (Exception e)
        {
            await webhookService.SendWebhookAsync(new()
            {
                Event = WebhookEvent.VideoTrackExtractionFailed,
                UserUuid = data.UserUuid,
                Error = e.Message
            }, cancellationToken);
            throw;
        }
    }
}
