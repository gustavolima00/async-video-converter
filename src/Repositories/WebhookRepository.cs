using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Repositories.Postgres;

namespace Repositories;

public interface IWebhookRepository
{
  Task<WebhookUser?> TryGetWebhookUserAsync(Guid userUuid);
  Task<WebhookUser> CreateWebhookUserAsync(string webhookUrl);
  Task<WebhookUser> CreateOrUpdateWebhookUserAsync(Guid uuid, string webhookUrl, IEnumerable<string> events);
  Task UpdateWebhookUserAsync(Guid uuid, string url, string webhookUrl);
  Task RegisterEventAsync(Guid uuid, string eventName);
  Task UnregisterEventAsync(Guid uuid, string eventName);
}

public class WebhookRepository : IWebhookRepository
{
  private readonly DatabaseContext _context;

  public WebhookRepository(DatabaseContext context)
  {
    _context = context;
  }

  public async Task<WebhookUser?> TryGetWebhookUserAsync(Guid userUuid)
  {
    return await _context.WebhookUsers.FirstOrDefaultAsync(wu => wu.Uuid == userUuid);
  }

  public async Task<WebhookUser> CreateWebhookUserAsync(string webhookUrl)
  {
    var webhookUser = new WebhookUser
    {
      WebhookUrl = webhookUrl
    };
    _context.WebhookUsers.Add(webhookUser);
    await _context.SaveChangesAsync();
    return webhookUser;
  }

  public async Task<WebhookUser> CreateOrUpdateWebhookUserAsync(Guid uuid, string webhookUrl, IEnumerable<string> events)
  {
    var webhookUser = await _context.WebhookUsers.FirstOrDefaultAsync(wu => wu.Uuid == uuid);
    if (webhookUser is null)
    {
      webhookUser = new WebhookUser
      {
        Uuid = uuid,
        WebhookUrl = webhookUrl,
        Events = events
      };
      _context.WebhookUsers.Add(webhookUser);
    }
    else
    {
      webhookUser.WebhookUrl = webhookUrl;
      webhookUser.Events = events;
    }
    await _context.SaveChangesAsync();
    return webhookUser;
  }

  public async Task UpdateWebhookUserAsync(Guid uuid, string url, string webhookUrl)
  {
    var webhookUser = await _context.WebhookUsers.FirstOrDefaultAsync(wu => wu.Uuid == uuid);
    if (webhookUser is not null)
    {
      webhookUser.WebhookUrl = webhookUrl;
      await _context.SaveChangesAsync();
    }
  }

  public async Task RegisterEventAsync(Guid uuid, string eventName)
  {
    var webhookUser = await _context.WebhookUsers.FirstOrDefaultAsync(wu => wu.Uuid == uuid);
    if (webhookUser is not null)
    {
      if (webhookUser.Events.Contains(eventName))
      {
        return;
      }

      webhookUser.Events = webhookUser.Events.Append(eventName);
      await _context.SaveChangesAsync();
    }
  }

  public async Task UnregisterEventAsync(Guid uuid, string eventName)
  {
    var webhookUser = await _context.WebhookUsers.FirstOrDefaultAsync(wu => wu.Uuid == uuid);
    if (webhookUser is not null)
    {
      webhookUser.Events = webhookUser.Events.Where(e => e != eventName);
      await _context.SaveChangesAsync();
    }
  }
}
