using System.Text;
using System.Text.Json;
using Repositories;
using Repositories.Models;
using Services.Models;

namespace Services;

public interface IWebhookService
{
  Task<WebhookUser> CreateWebhookUserAsync(string webhookUrl, CancellationToken cancellationToken = default);
  Task<WebhookUser> GetWebhookUserAsync(Guid userUuid, CancellationToken cancellationToken = default);
  Task ProcessWebhookAsync(WebHookDetails webHookDetails, CancellationToken cancellationToken = default);
  void SendWebhookAsync(WebHookDetails webHookDetails);
}

public class WebhookService : IWebhookService
{
  private readonly IWebhookRepository _webhookRepository;
  private readonly IQueueService _queueService;
  private readonly HttpClient _httpClient;

  public WebhookService(
    IWebhookRepository webhookRepository,
    HttpClient httpClient,
    IQueueService queueService
  )
  {
    _webhookRepository = webhookRepository;
    _httpClient = httpClient;
    _queueService = queueService;
  }

  public async Task<WebhookUser> CreateWebhookUserAsync(string webhookUrl, CancellationToken cancellationToken = default)
  {
    var webhookUser = await _webhookRepository.CreateWebhookUserAsync(webhookUrl);
    return webhookUser;
  }

  public async Task<WebhookUser> GetWebhookUserAsync(Guid userUuid, CancellationToken cancellationToken = default)
  {
    var webhookUser = await _webhookRepository.TryGetWebhookUserAsync(userUuid) ?? throw new Exception("Webhook user not found");
    return webhookUser;
  }

  public async Task ProcessWebhookAsync(WebHookDetails webHookDetails, CancellationToken cancellationToken = default)
  {
    var webhookUser = await GetWebhookUserAsync(webHookDetails.UserUuid, cancellationToken);
    var content = new StringContent(JsonSerializer.Serialize(webHookDetails), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync(webhookUser.WebhookUrl, content, cancellationToken);
    if (!response.IsSuccessStatusCode)
    {
      throw new Exception("Webhook failed to send");
    }
  }

  public void SendWebhookAsync(WebHookDetails webHookDetails)
  {
    _queueService.EnqueueWebhook(webHookDetails);
  }

}
