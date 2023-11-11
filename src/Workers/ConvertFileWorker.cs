using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Repositories.Models;
using Services;
using Services.Configuration;
using Services.Models;

namespace Workers;

public class ConvertFileWorker : BaseQueueWorker<FileToConvert>
{
    private readonly IQueueService _queueService;
    readonly string _queueUrl;
    public ConvertFileWorker(
        ILogger<ConvertFileWorker> logger,
        IQueueService queueService,
        QueuesConfiguration queuesConfiguration,
        IServiceScopeFactory serviceScopeFactory
    ) : base(logger, queueService, serviceScopeFactory)
    {
        _queueUrl = queuesConfiguration.ConvertQueueName;
        _queueService = queueService;
    }
    protected override string QueueUrl => _queueUrl;
    protected override Task ProcessMessage(IServiceScope scope, FileToConvert fileToConvert, CancellationToken cancellationToken)
    {
        var rawVideoService = scope.ServiceProvider.GetRequiredService<IRawVideoService>();
        var convertedVideoService = scope.ServiceProvider.GetRequiredService<IConvertedVideosService>();

        return fileToConvert.FileType switch
        {
            FileType.RawVideo =>
                ConvertRawVideoAsync(
                    rawVideoService,
                    convertedVideoService,
                    fileToConvert.Id,
                    cancellationToken
                ),
            FileType.RawSubtitle =>
                ConvertRawSubtitleAsync(
                    rawVideoService,
                    convertedVideoService,
                    fileToConvert.Id,
                    cancellationToken
                ),
            _ => throw new NotImplementedException()
        };
    }

    private async Task ConvertRawVideoAsync(
        IRawVideoService rawVideoService,
        IConvertedVideosService convertedVideoService,
        int id,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var rawFile = await rawVideoService.GetRawVideoAsync(id, cancellationToken);
            await rawVideoService.UpdateRawVideoConversionStatus(id, ConversionStatus.Converting, cancellationToken);
            var stream = await rawVideoService.ConvertRawVideoToMp4Async(id, cancellationToken);
            await convertedVideoService.SaveConvertedVideoAsync(stream, id, cancellationToken);
            await rawVideoService.UpdateRawVideoConversionStatus(id, ConversionStatus.Converted, cancellationToken);
            // _queueService.EnqueueWebhook(new()
            // {
            //     UserUuid = rawFile.UserUuid,
            //     WebhookType = WebhookType.VideoConversionFinished,
            // });
        }
        catch
        {
            await rawVideoService.UpdateRawVideoConversionStatus(id, ConversionStatus.Error, cancellationToken);
            throw;
        }
    }

    private async Task ConvertRawSubtitleAsync(
        IRawVideoService rawVideoService,
        IConvertedVideosService convertedVideoService,
        int id,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var rawSubtitle = await rawVideoService.GetRawSubtitleAsync(id, cancellationToken);
            await rawVideoService.UpdateRawSubtitleConversionStatus(id, ConversionStatus.Converting, cancellationToken);
            var stream = await rawVideoService.ConvertRawSubtitleToVttAsync(id, cancellationToken);
            await convertedVideoService.SaveConvertedSubtitleAsync(stream, id, cancellationToken);
            await rawVideoService.UpdateRawSubtitleConversionStatus(id, ConversionStatus.Converted, cancellationToken);
            // _queueService.EnqueueWebhook(new()
            // {
            //     UserUuid = rawSubtitle.UserUuid,
            //     WebhookType = WebhookType.SubtitleConversionFinished,
            // });
        }
        catch
        {
            await rawVideoService.UpdateRawSubtitleConversionStatus(id, ConversionStatus.Error, cancellationToken);
            throw;
        }
    }
}
