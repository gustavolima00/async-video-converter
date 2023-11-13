using System.Text;
using System.Text.Json;
using Repositories;
using Repositories.Models;
using Services.Models;

namespace Services;

public interface IWebhookService
{
  Task<WebhookUser> CreateWebhookUserAsync(
    string webhookUrl,
    CancellationToken cancellationToken = default
  );
  Task<WebhookUser> GetWebhookUserAsync(
    Guid userUuid,
    CancellationToken cancellationToken = default
  );
  Task<WebhookUser> CreateOrUpdateWebhookUserAsync(
    Guid uuid,
    string webhookUrl,
    IEnumerable<string> events,
    CancellationToken cancellationToken = default
  );
  Task RegisterEventAsync(
    Guid uuid,
    string eventName,
    CancellationToken cancellationToken = default
  );
  Task UnregisterEventAsync(
    Guid uuid,
    string eventName,
    CancellationToken cancellationToken = default
  );
  Task ProcessWebhookAsync(
    WebHookDetails webHookDetails,
    CancellationToken cancellationToken = default
  );
  Task SendWebhookAsync(
    WebHookDetails webHookDetails,
    CancellationToken cancellationToken = default
  );
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

  public async Task<WebhookUser> CreateWebhookUserAsync(
    string webhookUrl,
    CancellationToken cancellationToken = default
  )
  {
    var webhookUser = await _webhookRepository.CreateWebhookUserAsync(webhookUrl, cancellationToken);
    return webhookUser;
  }

  public async Task<WebhookUser> GetWebhookUserAsync(
    Guid userUuid,
    CancellationToken cancellationToken = default
  )
  {
    var webhookUser = await _webhookRepository.TryGetWebhookUserAsync(userUuid, cancellationToken) ?? throw new Exception("Webhook user not found");
    return webhookUser;
  }

  public async Task<WebhookUser> CreateOrUpdateWebhookUserAsync(
    Guid uuid,
    string webhookUrl,
    IEnumerable<string> events,
    CancellationToken cancellationToken = default
  )
  {
    var webhookUser = await _webhookRepository.CreateOrUpdateWebhookUserAsync(uuid, webhookUrl, events, cancellationToken);
    return webhookUser;
  }

  public async Task RegisterEventAsync(
    Guid uuid,
    string eventName,
    CancellationToken cancellationToken = default
  )
  {
    await _webhookRepository.RegisterEventAsync(uuid, eventName, cancellationToken);
  }

  public async Task UnregisterEventAsync(
    Guid uuid,
    string eventName,
    CancellationToken cancellationToken = default
  )
  {
    await _webhookRepository.UnregisterEventAsync(uuid, eventName, cancellationToken);
  }

  public async Task ProcessWebhookAsync(
    WebHookDetails webHookDetails,
    CancellationToken cancellationToken = default
  )
  {
    var webhookUser = await GetWebhookUserAsync(webHookDetails.UserUuid, cancellationToken);
    var content = new StringContent(JsonSerializer.Serialize(webHookDetails), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync(webhookUser.WebhookUrl, content, cancellationToken);
    if (!response.IsSuccessStatusCode)
    {
      throw new Exception("Webhook failed to send");
    }
  }

  public async Task SendWebhookAsync(
    WebHookDetails webHookDetails,
    CancellationToken cancellationToken = default
  )
  {
    var webhookUser = await GetWebhookUserAsync(webHookDetails.UserUuid, cancellationToken);
    if (webhookUser.Events.Contains(webHookDetails.Event.ToString()))
    {
      _queueService.EnqueueWebhook(webHookDetails);
    }
  }
}
