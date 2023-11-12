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
    public ConvertFileWorker(
        ILogger<ConvertFileWorker> logger,
        IQueueService queueService,
        QueuesConfiguration queuesConfiguration,
        IServiceScopeFactory serviceScopeFactory
    ) : base(logger, queueService, serviceScopeFactory)
    {
        _queueUrl = queuesConfiguration.ConvertQueueName;
    }
    protected override string QueueUrl => _queueUrl;
    protected override Task ProcessMessage(IServiceScope scope, FileToConvert fileToConvert, CancellationToken cancellationToken)
    {
        var rawVideosService = scope.ServiceProvider.GetRequiredService<IRawVideoService>();
        var rawSubtitlesService = scope.ServiceProvider.GetRequiredService<IRawSubtitlesService>();
        var convertedVideoService = scope.ServiceProvider.GetRequiredService<IConvertedVideosService>();

        return fileToConvert.FileType switch
        {
            FileType.RawVideo =>
                ConvertRawVideoAsync(
                    rawVideosService,
                    convertedVideoService,
                    fileToConvert.Id,
                    cancellationToken
                ),
            FileType.RawSubtitle =>
                ConvertRawSubtitleAsync(
                    rawSubtitlesService,
                    convertedVideoService,
                    fileToConvert.Id,
                    cancellationToken
                ),
            _ => throw new NotImplementedException()
        };
    }

    private static async Task ConvertRawVideoAsync(
        IRawVideoService rawVideosService,
        IConvertedVideosService convertedVideoService,
        int id,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var rawFile = await rawVideosService.GetAsync(id, cancellationToken);
            await rawVideosService.UpdateConversionStatusAsync(id, ConversionStatus.Converting, cancellationToken);
            var stream = await rawVideosService.ConvertToMp4Async(id, cancellationToken);
            await convertedVideoService.SaveConvertedVideoAsync(stream, id, cancellationToken);
            await rawVideosService.UpdateConversionStatusAsync(id, ConversionStatus.Converted, cancellationToken);
            // _queueService.EnqueueWebhook(new()
            // {
            //     UserUuid = rawFile.UserUuid,
            //     WebhookType = WebhookType.VideoConversionFinished,
            // });
        }
        catch
        {
            await rawVideosService.UpdateConversionStatusAsync(id, ConversionStatus.Error, cancellationToken);
            throw;
        }
    }

    private static async Task ConvertRawSubtitleAsync(
        IRawSubtitlesService rawSubtitlesService,
        IConvertedVideosService convertedVideoService,
        int id,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var rawSubtitle = await rawSubtitlesService.GetRawSubtitleAsync(id, cancellationToken);
            await rawSubtitlesService.UpdateRawSubtitleConversionStatus(id, ConversionStatus.Converting, cancellationToken);
            var stream = await rawSubtitlesService.ConvertRawSubtitleToVttAsync(id, cancellationToken);
            await convertedVideoService.SaveConvertedSubtitleAsync(stream, id, cancellationToken);
            await rawSubtitlesService.UpdateRawSubtitleConversionStatus(id, ConversionStatus.Converted, cancellationToken);
            // _queueService.EnqueueWebhook(new()
            // {
            //     UserUuid = rawSubtitle.UserUuid,
            //     WebhookType = WebhookType.SubtitleConversionFinished,
            // });
        }
        catch
        {
            await rawSubtitlesService.UpdateRawSubtitleConversionStatus(id, ConversionStatus.Error, cancellationToken);
            throw;
        }
    }
}
