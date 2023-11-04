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

    protected BaseQueueWorker(ILogger<BaseQueueWorker<TMessageType>> logger, IQueueService queueService)
    {
        _logger = logger;
        _queueService = queueService;
    }

    protected abstract string QueueUrl { get; }
    protected virtual int DelayAfterNoMessage => 1000;
    protected virtual int DelayAfterError => 1000;
    protected virtual int MaxParallel => 4;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var messages = _queueService.ReadMessages<TMessageType>(QueueUrl, MaxParallel);
                if (messages.Any())
                {
                    await ProcessMessages(messages, cancellationToken);
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

    private async Task ProcessMessages(IEnumerable<(ulong messageId, TMessageType message)> messages, CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            messages.Select(
                (tuple) =>
                    LogAndProcessMessage(tuple.messageId, tuple.message, cancellationToken)
                    ).ToList());
    }

    private async Task LogAndProcessMessage(ulong messageId, TMessageType message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Processing message: {messageId}");
        try
        {
            await ProcessMessage(message, cancellationToken);
            _logger.LogInformation("Message {@message} processed ", messageId);
            _queueService.DeleteMessage(messageId);
        }
        catch (Exception e)
        {
            _logger.LogError($"Error processing message: {e.Message}");
        }
    }
    protected abstract Task ProcessMessage(TMessageType message, CancellationToken cancellationToken);
}
