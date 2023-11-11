using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services;
using Services.Configuration;
using Services.Models;

namespace Workers;

public class WebhookWorker : BaseQueueWorker<WebHookDetails>
{
    readonly string _queueUrl;
    public WebhookWorker(
        ILogger<WebhookWorker> logger,
        IQueueService queueService,
        QueuesConfiguration queuesConfiguration,
        IServiceScopeFactory serviceScopeFactory
    ) : base(logger, queueService, serviceScopeFactory)
    {
        _queueUrl = queuesConfiguration.WebhookQueueName;
    }
    protected override string QueueUrl => _queueUrl;
    protected override async Task ProcessMessage(IServiceScope scope, WebHookDetails webHookDetails, CancellationToken cancellationToken)
    {
        var webhookService = scope.ServiceProvider.GetRequiredService<IWebhookService>();
        await webhookService.SendWebhookAsync(webHookDetails, cancellationToken);
    }
}
