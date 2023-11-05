using Microsoft.Extensions.Logging;
using Repositories.Models;
using Services;
using Services.Configuration;

namespace Workers;

public class ConvertFileWorker : BaseQueueWorker<int>
{
    private readonly IRawFilesService _fileStorageService;
    private readonly IWebVideoService _webVideoService;
    readonly string _queueUrl;
    public ConvertFileWorker(
        ILogger<ConvertFileWorker> logger,
        IQueueService queueService,
        IRawFilesService fileStorageService,
        QueuesConfiguration queuesConfiguration,
        IWebVideoService webVideoService
    ) : base(logger, queueService)
    {
        _fileStorageService = fileStorageService;
        _queueUrl = queuesConfiguration.ConvertQueueName;
        _webVideoService = webVideoService;
    }
    protected override string QueueUrl => _queueUrl;
    protected override async Task ProcessMessage(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _fileStorageService.UpdateConversionStatus(id, ConversionStatus.Converting, cancellationToken);
            var stream = await _fileStorageService.ConvertToMp4(id, cancellationToken);
            await _webVideoService.SaveWebVideoAsync(stream, id, cancellationToken);
            await _fileStorageService.UpdateConversionStatus(id, ConversionStatus.Converted, cancellationToken);
        }
        catch
        {
            await _fileStorageService.UpdateConversionStatus(id, ConversionStatus.Error, cancellationToken);
            throw;
        }
    }
}
