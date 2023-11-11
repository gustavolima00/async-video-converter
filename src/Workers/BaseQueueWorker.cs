using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Services;

namespace Workers;

public abstract class BaseQueueWorker<TMessageType> : BackgroundService
{
    private class MessageShape
    {
        public string MessageId { get; set; } = "";
        public string Message { get; set; } = "";
    }

    private readonly ILogger<BaseQueueWorker<TMessageType>> _logger;
    private readonly IQueueService _queueService;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    protected BaseQueueWorker(ILogger<BaseQueueWorker<TMessageType>> logger, IQueueService queueService, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _queueService = queueService;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected abstract string QueueUrl { get; }
    protected virtual int DelayAfterNoMessage => 1000;
    protected virtual int DelayAfterError => 1000;
    protected virtual int BatchSize => 10;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var messages = _queueService.ReadMessages<TMessageType>(QueueUrl, BatchSize);
                if (messages.Any())
                {
                    foreach (var message in messages)
                    {
                        await LogAndProcessMessage(scope, message.messageId, message.message, cancellationToken);
                    }
                    continue;
                }
                await Task.Delay(DelayAfterNoMessage, cancellationToken);

            }
            catch (Exception e)
            {
                _logger.LogError("Error receiving message from queue: {@response}", e);
                await Task.Delay(DelayAfterError, cancellationToken);
                continue;
            }
        }
    }

    private async Task LogAndProcessMessage(IServiceScope scope, ulong messageId, TMessageType message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Processing message: {messageId}");
        try
        {
            await ProcessMessage(scope, message, cancellationToken);
            _logger.LogInformation("Message {@message} processed ", messageId);
            _queueService.DeleteMessage(messageId);
        }
        catch (Exception e)
        {
            _logger.LogError($"Error processing message: {e.Message}");
        }
    }
    protected abstract Task ProcessMessage(IServiceScope scope, TMessageType message, CancellationToken cancellationToken);
}
