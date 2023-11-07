using Microsoft.Extensions.Logging;
using Repositories.Models;
using Services;
using Services.Configuration;
using Services.Models;

namespace Workers;

public class ConvertFileWorker : BaseQueueWorker<int>
{
    private readonly IRawVideoService _fileStorageService;
    private readonly IConvertedVideosService _webVideoService;
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
        _fileStorageService = fileStorageService;
        _queueUrl = queuesConfiguration.ConvertQueueName;
        _webVideoService = webVideoService;
        _queueService = queueService;
    }
    protected override string QueueUrl => _queueUrl;
    protected override async Task ProcessMessage(int id, CancellationToken cancellationToken)
    {
        try
        {
            var rawFile = await _fileStorageService.GetRawVideoAsync(id, cancellationToken);
            await _fileStorageService.UpdateRawVideoConversionStatus(id, ConversionStatus.Converting, cancellationToken);
            var stream = await _fileStorageService.ConvertRawVideoToMp4Async(id, cancellationToken);
            await _webVideoService.SaveConvertedVideoAsync(stream, id, cancellationToken);
            await _fileStorageService.UpdateRawVideoConversionStatus(id, ConversionStatus.Converted, cancellationToken);
            _queueService.EnqueueWebhook(new()
            {
                UserUuid = rawFile.UserUuid,
                WebhookType = WebhookType.VideoConversionFinished,
            });
        }
        catch
        {
            await _fileStorageService.UpdateRawVideoConversionStatus(id, ConversionStatus.Error, cancellationToken);
            throw;
        }
    }
}
