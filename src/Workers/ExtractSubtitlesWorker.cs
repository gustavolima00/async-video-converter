using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Repositories.Models;
using Services;
using Services.Configuration;
using Services.Models;

namespace Workers;

public class ExtractSubtitlesWorker : BaseQueueWorker<VideoToExtractSubtitles>
{
    private readonly string _queueUrl;
    private readonly ILogger<ExtractSubtitlesWorker> _logger;
    public ExtractSubtitlesWorker(
        ILogger<ExtractSubtitlesWorker> logger,
        QueuesConfiguration queuesConfiguration,
        IServiceScopeFactory serviceScopeFactory
    ) : base(logger, serviceScopeFactory)
    {
        _queueUrl = queuesConfiguration.ExtractSubtitlesQueueName;
        _logger = logger;
    }
    protected override string QueueUrl => _queueUrl;
    protected override async Task ProcessMessage(
        IServiceScope scope,
        VideoToExtractSubtitles videoToExtractSubtitles,
        CancellationToken cancellationToken
    )
    {
        var videoConversionService = scope.ServiceProvider.GetRequiredService<IVideoConversionService>();
        var rawVideoService = scope.ServiceProvider.GetRequiredService<IRawVideoService>();
        var webhookService = scope.ServiceProvider.GetRequiredService<IWebhookService>();
        try
        {
            await rawVideoService.UpdateSubtitleExtractionStatus(videoToExtractSubtitles.RawVideoId, AsyncTaskStatus.Running, cancellationToken);
            await videoConversionService.ExtractSubtitlesAsync(videoToExtractSubtitles.RawVideoId, cancellationToken);
            var rawVideo = await rawVideoService.GetAsync(videoToExtractSubtitles.RawVideoId, cancellationToken);
            var subtitles = rawVideo.ConvertedVideo.Subtitles.ToList();
            await webhookService.SendWebhookAsync<SubtitleExtractionWebhook>(new()
            {
                Event = WebhookEvent.SubtitleTracksExtracted,
                UserUuid = videoToExtractSubtitles.UserUuid,
                Payload = new()
                {
                    RawVideoUuid = videoToExtractSubtitles.RawVideoUuid,
                    UserUuid = videoToExtractSubtitles.UserUuid,
                    Subtitles = subtitles.Select(x => new SubtitleDetails
                    {
                        Language = x.Language,
                        Path = x.Path
                    })
                }
            }, cancellationToken);
            await rawVideoService.UpdateSubtitleExtractionStatus(videoToExtractSubtitles.RawVideoId, AsyncTaskStatus.Completed, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to extract subtitle tracks");
            await webhookService.SendWebhookAsync<SubtitleExtractionWebhook>(new()
            {
                Event = WebhookEvent.SubtitleTrackExtractionFailed,
                UserUuid = videoToExtractSubtitles.UserUuid,
                Error = e.Message,
                Payload = new()
                {
                    RawVideoUuid = videoToExtractSubtitles.RawVideoUuid,
                    UserUuid = videoToExtractSubtitles.UserUuid,
                }
            }, cancellationToken);
            await rawVideoService.UpdateSubtitleExtractionStatus(videoToExtractSubtitles.RawVideoId, AsyncTaskStatus.Failed, cancellationToken);
        }
    }
}
