using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Repositories.Models;
using Services;
using Services.Configuration;
using Services.Models;

namespace Workers;

public class ConvertFileWorker : BaseQueueWorker<FileToConvert>
{
    readonly string _queueUrl;
    ILogger<ConvertFileWorker> _logger;

    public ConvertFileWorker(
        ILogger<ConvertFileWorker> logger,
        IQueueService queueService,
        QueuesConfiguration queuesConfiguration,
        IServiceScopeFactory serviceScopeFactory
    ) : base(logger, queueService, serviceScopeFactory)
    {
        _queueUrl = queuesConfiguration.ConvertQueueName;
        _logger = logger;
    }
    protected override string QueueUrl => _queueUrl;
    protected override Task ProcessMessage(IServiceScope scope, FileToConvert fileToConvert, CancellationToken cancellationToken)
    {
        var rawVideosService = scope.ServiceProvider.GetRequiredService<IRawVideoService>();
        var rawSubtitlesService = scope.ServiceProvider.GetRequiredService<IRawSubtitlesService>();
        var convertedVideoService = scope.ServiceProvider.GetRequiredService<IConvertedVideosService>();
        var convertedSubtitleService = scope.ServiceProvider.GetRequiredService<IConvertedSubtitleService>();
        var webhookService = scope.ServiceProvider.GetRequiredService<IWebhookService>();

        return fileToConvert.FileType switch
        {
            FileType.RawVideo =>
                ConvertRawVideoAsync(
                    rawVideosService,
                    convertedVideoService,
                    webhookService,
                    fileToConvert.Id,
                    cancellationToken
                ),
            FileType.RawSubtitle =>
                ConvertRawSubtitleAsync(
                    rawSubtitlesService,
                    convertedSubtitleService,
                    webhookService,
                    fileToConvert.Id,
                    cancellationToken
                ),
            _ => throw new NotImplementedException()
        };
    }

    private async Task ConvertRawVideoAsync(
        IRawVideoService rawVideosService,
        IConvertedVideosService convertedVideoService,
        IWebhookService webhookService,
        int id,
        CancellationToken cancellationToken = default
    )
    {
        var rawFile = await rawVideosService.GetAsync(id, cancellationToken);
        try
        {
            await rawVideosService.UpdateConversionStatusAsync(id, ConversionStatus.Converting, cancellationToken);
            var stream = await rawVideosService.ConvertToMp4Async(id, cancellationToken);
            await convertedVideoService.SaveConvertedVideoAsync(stream, id, cancellationToken);
            await rawVideosService.UpdateConversionStatusAsync(id, ConversionStatus.Converted, cancellationToken);
            await webhookService.SendWebhookAsync(
                new()
                {
                    UserUuid = rawFile.UserUuid,
                    Event = WebhookEvent.VideoConversionFinished,
                },
                cancellationToken
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error converting video");
            await rawVideosService.UpdateConversionStatusAsync(id, ConversionStatus.Error, cancellationToken);
            await webhookService.SendWebhookAsync(
                new()
                {
                    UserUuid = rawFile.UserUuid,
                    Event = WebhookEvent.VideoConversionError,
                },
                cancellationToken
            );
        }
    }

    private async Task ConvertRawSubtitleAsync(
        IRawSubtitlesService rawSubtitlesService,
        IConvertedSubtitleService convertedSubtitleService,
        IWebhookService webhookService,
        int id,
        CancellationToken cancellationToken = default
    )
    {
        var rawSubtitle = await rawSubtitlesService.GetAsync(id, cancellationToken);
        try
        {
            await rawSubtitlesService.UpdateConversionStatusAsync(id, ConversionStatus.Converting, cancellationToken);
            var stream = await rawSubtitlesService.ConvertToVttAsync(id, cancellationToken);
            await convertedSubtitleService.SaveConvertedSubtitleAsync(stream, id, cancellationToken);
            await rawSubtitlesService.UpdateConversionStatusAsync(id, ConversionStatus.Converted, cancellationToken);
            await webhookService.SendWebhookAsync(
                new()
                {
                    UserUuid = rawSubtitle.UserUuid,
                    Event = WebhookEvent.SubtitleConversionFinished,
                },
                cancellationToken
            );
        }
        catch(Exception e)
        {
            _logger.LogError(e, "Error converting subtitle");
            await rawSubtitlesService.UpdateConversionStatusAsync(id, ConversionStatus.Error, cancellationToken);
            await webhookService.SendWebhookAsync(
                new()
                {
                    UserUuid = rawSubtitle.UserUuid,
                    Event = WebhookEvent.SubtitleConversionError,
                },
                cancellationToken
            );
        }
    }
}
