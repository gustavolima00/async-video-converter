using Microsoft.Extensions.Logging;
using Repositories.Models;
using Services;
using Services.Configuration;
using Services.Models;

namespace Workers;

public class ConvertFileWorker : BaseQueueWorker<FileToConvert>
{
    private readonly IRawVideoService _rawVideoService;
    private readonly IConvertedVideosService _convertedVideoService;
    private readonly IQueueService _queueService;
    readonly string _queueUrl;
    public ConvertFileWorker(
        ILogger<ConvertFileWorker> logger,
        IQueueService queueService,
        IRawVideoService fileStorageService,
        QueuesConfiguration queuesConfiguration,
        IConvertedVideosService webVideoService
    ) : base(logger, queueService)
    {
        _rawVideoService = fileStorageService;
        _queueUrl = queuesConfiguration.ConvertQueueName;
        _convertedVideoService = webVideoService;
        _queueService = queueService;
    }
    protected override string QueueUrl => _queueUrl;
    protected override Task ProcessMessage(FileToConvert fileToConvert, CancellationToken cancellationToken)
    {

        return fileToConvert.FileType switch
        {
            FileType.RawVideo => ConvertRawVideoAsync(fileToConvert.Id, cancellationToken),
            FileType.RawSubtitle => ConvertRawSubtitleAsync(fileToConvert.Id, cancellationToken),
            _ => throw new NotImplementedException()
        };
    }

    private async Task ConvertRawVideoAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var rawFile = await _rawVideoService.GetRawVideoAsync(id, cancellationToken);
            await _rawVideoService.UpdateRawVideoConversionStatus(id, ConversionStatus.Converting, cancellationToken);
            var stream = await _rawVideoService.ConvertRawVideoToMp4Async(id, cancellationToken);
            await _convertedVideoService.SaveConvertedVideoAsync(stream, id, cancellationToken);
            await _rawVideoService.UpdateRawVideoConversionStatus(id, ConversionStatus.Converted, cancellationToken);
            _queueService.EnqueueWebhook(new()
            {
                UserUuid = rawFile.UserUuid,
                WebhookType = WebhookType.VideoConversionFinished,
            });
        }
        catch
        {
            await _rawVideoService.UpdateRawVideoConversionStatus(id, ConversionStatus.Error, cancellationToken);
            throw;
        }
    }

    private async Task ConvertRawSubtitleAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var rawSubtitle = await _rawVideoService.GetRawSubtitleAsync(id, cancellationToken);
            await _rawVideoService.UpdateRawSubtitleConversionStatus(id, ConversionStatus.Converting, cancellationToken);
            var stream = await _rawVideoService.ConvertRawSubtitleToVttAsync(id, cancellationToken);
            await _convertedVideoService.SaveConvertedSubtitleAsync(stream, id, cancellationToken);
            await _rawVideoService.UpdateRawSubtitleConversionStatus(id, ConversionStatus.Converted, cancellationToken);
            _queueService.EnqueueWebhook(new()
            {
                UserUuid = rawSubtitle.UserUuid,
                WebhookType = WebhookType.SubtitleConversionFinished,
            });
        }
        catch
        {
            await _rawVideoService.UpdateRawSubtitleConversionStatus(id, ConversionStatus.Error, cancellationToken);
            throw;
        }
    }
}
