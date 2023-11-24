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
    private readonly IServiceScopeFactory _serviceScopeFactory;

    protected BaseQueueWorker(ILogger<BaseQueueWorker<TMessageType>> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected abstract string QueueUrl { get; }
    protected virtual int DelayAfterNoMessage => 1000;
    protected virtual int DelayAfterError => 1000;
    protected virtual int BatchSize => 4;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var queueService = scope.ServiceProvider.GetRequiredService<IQueueService>();
                var messages = queueService.ReadMessages<TMessageType>(QueueUrl, BatchSize);
                if (messages.Any())
                {
                    var tasks = messages.Select(async message =>
                    {

                        await LogAndProcessMessage(scope, queueService, message.messageId, message.message, cancellationToken);
                    });
                    await Task.WhenAll(tasks);
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

    private async Task LogAndProcessMessage(
        IServiceScope scope,
        IQueueService queueService,
        ulong messageId,
        TMessageType message,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation($"Processing message: {messageId}");
        try
        {
            await ProcessMessage(scope, message, cancellationToken);
            _logger.LogInformation("Message {@message} processed ", messageId);
            queueService.DeleteMessage(messageId);
        }
        catch (Exception e)
        {
            _logger.LogError($"Error processing message: {e.Message}");
        }
    }
    protected abstract Task ProcessMessage(IServiceScope scope, TMessageType message, CancellationToken cancellationToken);
}
