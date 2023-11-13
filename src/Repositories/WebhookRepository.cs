using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Repositories.Postgres;

namespace Repositories;

public interface IWebhookRepository
{
  Task<WebhookUser?> TryGetWebhookUserAsync(
    Guid userUuid,
    CancellationToken cancellationToken = default
  );
  Task<WebhookUser> CreateWebhookUserAsync(
    string webhookUrl,
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
}

public class WebhookRepository : IWebhookRepository
{
  private readonly DatabaseContext _context;

  public WebhookRepository(DatabaseContext context)
  {
    _context = context;
  }

  public async Task<WebhookUser?> TryGetWebhookUserAsync(
    Guid userUuid,
    CancellationToken cancellationToken = default
  )
  {
    return await _context.WebhookUsers.FirstOrDefaultAsync(wu => wu.Uuid == userUuid, cancellationToken: cancellationToken);
  }

  public async Task<WebhookUser> CreateWebhookUserAsync(
    string webhookUrl,
    CancellationToken cancellationToken = default
  )
  {
    var webhookUser = new WebhookUser
    {
      WebhookUrl = webhookUrl
    };
    _context.WebhookUsers.Add(webhookUser);
    await _context.SaveChangesAsync(cancellationToken);
    return webhookUser;
  }

  public async Task<WebhookUser> CreateOrUpdateWebhookUserAsync(
    Guid uuid,
    string webhookUrl,
    IEnumerable<string> events,
    CancellationToken cancellationToken = default
  )
  {
    var webhookUser = await _context.WebhookUsers.FirstOrDefaultAsync(wu => wu.Uuid == uuid, cancellationToken: cancellationToken);
    if (webhookUser is null)
    {
      webhookUser = new WebhookUser
      {
        Uuid = uuid,
        WebhookUrl = webhookUrl,
        Events = events.ToList()
      };
      _context.WebhookUsers.Add(webhookUser);
    }
    else
    {
      webhookUser.WebhookUrl = webhookUrl;
      webhookUser.Events = events.ToList();
    }
    await _context.SaveChangesAsync(cancellationToken);
    return webhookUser;
  }

  public async Task RegisterEventAsync(
    Guid uuid,
    string eventName,
    CancellationToken cancellationToken = default
  )
  {
    var webhookUser = await _context.WebhookUsers.FirstOrDefaultAsync(wu => wu.Uuid == uuid, cancellationToken: cancellationToken);
    if (webhookUser is not null)
    {
      if (webhookUser.Events.Contains(eventName))
      {
        return;
      }

      webhookUser.Events.Add(eventName);
      await _context.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task UnregisterEventAsync(
    Guid uuid,
    string eventName,
    CancellationToken cancellationToken = default
  )
  {
    var webhookUser = await _context.WebhookUsers.FirstOrDefaultAsync(wu => wu.Uuid == uuid, cancellationToken: cancellationToken);
    if (webhookUser is not null)
    {
      webhookUser.Events.Remove(eventName);
      await _context.SaveChangesAsync(cancellationToken);
    }
  }
}
