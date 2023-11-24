using System.Text;
using Repositories;
using Repositories.Models;
using Services.Exceptions;
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
    WebHookToEnqueue webHookToEnqueue,
    CancellationToken cancellationToken = default
  );
  Task SendWebhookAsync<T>(
    WebHookDetails<T> webHookDetails,
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
    WebHookToEnqueue webHookToEnqueue,
    CancellationToken cancellationToken = default
  )
  {
    var content = new StringContent(webHookToEnqueue.SerializedData, Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync(webHookToEnqueue.Url, content, cancellationToken);
    if (!response.IsSuccessStatusCode)
    {
      throw new SendWebhookException(webHookToEnqueue.Url, webHookToEnqueue.SerializedData, response);
    }
  }

  public async Task SendWebhookAsync<T>(
    WebHookDetails<T> webHookDetails,
    CancellationToken cancellationToken = default
  )
  {
    var webhookUser = await GetWebhookUserAsync(webHookDetails.UserUuid, cancellationToken);
    if (webhookUser.Events.Contains(webHookDetails.Event.ToString()))
    {
      _queueService.EnqueueWebhook(webHookDetails, webhookUser.WebhookUrl);
    }
  }
}
