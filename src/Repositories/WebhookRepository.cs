using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Repositories.Postgres;

namespace Repositories;

public interface IWebhookRepository
{
  Task<WebhookUser?> TryGetWebhookUserAsync(int id);
  Task<WebhookUser> CreateWebhookUserAsync(string webhookUrl);
  Task UpdateWebhookUserAsync(Guid uuid, string url, string webhookUrl);
}

public class WebhookRepository : IWebhookRepository
{
  private readonly DatabaseContext _context;

  public WebhookRepository(DatabaseContext context)
  {
    _context = context;
  }

  public async Task<WebhookUser?> TryGetWebhookUserAsync(int id)
  {
    return await _context.WebhookUsers.FindAsync(id);
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

  public async Task UpdateWebhookUserAsync(Guid uuid, string url, string webhookUrl)
  {
    var webhookUser = await _context.WebhookUsers.FirstOrDefaultAsync(wu => wu.Uuid == uuid);
    if (webhookUser is not null)
    {
      webhookUser.WebhookUrl = webhookUrl;
      await _context.SaveChangesAsync();
    }
  }
}
