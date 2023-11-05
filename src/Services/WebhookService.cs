using Repositories;
using Repositories.Models;

namespace Services;

public interface IWebhookService
{
  Task<WebhookUser> CreateWebhookUserAsync(string webhookUrl, CancellationToken cancellationToken = default);
}

public class WebhookService : IWebhookService
{
  private readonly IWebhookRepository _webhookRepository;

  public WebhookService(IWebhookRepository webhookRepository)
  {
    _webhookRepository = webhookRepository;
  }

  public async Task<WebhookUser> CreateWebhookUserAsync(string webhookUrl, CancellationToken cancellationToken = default)
  {
    var webhookUser = await _webhookRepository.CreateWebhookUserAsync(webhookUrl);
    return webhookUser;
  }

}
