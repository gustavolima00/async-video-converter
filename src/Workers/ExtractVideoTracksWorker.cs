using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Repositories.Models;
using Services;
using Services.Configuration;
using Services.Models;

namespace Workers;

public class ExtractVideoTracksWorker : BaseQueueWorker<VideoToExtractVideoTracks>
{
    private readonly string _queueUrl;
    private readonly ILogger<ExtractVideoTracksWorker> _logger;
    public ExtractVideoTracksWorker(
        ILogger<ExtractVideoTracksWorker> logger,
        IQueueService queueService,
        QueuesConfiguration queuesConfiguration,
        IServiceScopeFactory serviceScopeFactory
    ) : base(logger, queueService, serviceScopeFactory)
    {
        _queueUrl = queuesConfiguration.ExtractVideoTracksQueueName;
        _logger = logger;
    }
    protected override string QueueUrl => _queueUrl;
    protected override async Task ProcessMessage(
        IServiceScope scope,
        VideoToExtractVideoTracks data,
        CancellationToken cancellationToken
    )
    {
        var videoConversionService = scope.ServiceProvider.GetRequiredService<IVideoConversionService>();
        var rawVideoService = scope.ServiceProvider.GetRequiredService<IRawVideoService>();
        var webhookService = scope.ServiceProvider.GetRequiredService<IWebhookService>();
        try
        {
            await rawVideoService.UpdateTrackExtractionStatus(data.RawVideoId, AsyncTaskStatus.Running, cancellationToken);
            await videoConversionService.ExtractVideoTracksAndConvertAsync(data.RawVideoId, cancellationToken);
            await rawVideoService.UpdateTrackExtractionStatus(data.RawVideoId, AsyncTaskStatus.Completed, cancellationToken);
            var rawVideo = await rawVideoService.GetAsync(data.RawVideoId, cancellationToken);
            var videoTracks = rawVideo.ConvertedVideo.Streams.ToList();
            await webhookService.SendWebhookAsync<TrackExtranctionWebhook>(new()
            {
                Event = WebhookEvent.VideoTracksExtracted,
                UserUuid = data.UserUuid,
                Payload = new()
                {
                    RawVideoUuid = data.RawVideoUuid,
                    UserUuid = data.UserUuid,
                    VideoTracks = videoTracks.Select(x => new TrackDetails
                    {
                        Path = x.Path,
                        Language = x.Language,
                    })
                }
            }, cancellationToken);

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to extract video tracks");
            await webhookService.SendWebhookAsync<TrackExtranctionWebhook>(new()
            {
                Event = WebhookEvent.VideoTrackExtractionFailed,
                UserUuid = data.UserUuid,
                Error = e.Message,
                Payload = new()
                {
                    RawVideoUuid = data.RawVideoUuid,
                    UserUuid = data.UserUuid,
                }
            }, cancellationToken);
            await rawVideoService.UpdateTrackExtractionStatus(data.RawVideoId, AsyncTaskStatus.Failed, cancellationToken);
        }
    }
}
