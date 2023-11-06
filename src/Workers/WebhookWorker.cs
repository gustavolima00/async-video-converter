using Microsoft.Extensions.Logging;
using Services;
using Services.Configuration;
using Services.Models;

namespace Workers;

public class WebhookWorker : BaseQueueWorker<WebHookDetails>
{
    private readonly IWebhookService _webhookService;
    readonly string _queueUrl;
    public WebhookWorker(
        ILogger<WebhookWorker> logger,
        IQueueService queueService,
        QueuesConfiguration queuesConfiguration,
        IWebhookService webhookService
    ) : base(logger, queueService)
    {
        _queueUrl = queuesConfiguration.ConvertQueueName;
        _webhookService = webhookService;
    }
    protected override string QueueUrl => _queueUrl;
    protected override async Task ProcessMessage(WebHookDetails webHookDetails, CancellationToken cancellationToken)
    {
        await _webhookService.SendWebhookAsync(webHookDetails, cancellationToken);
    }
}
