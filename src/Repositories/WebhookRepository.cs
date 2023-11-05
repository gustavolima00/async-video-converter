using Repositories.Models;
using Repositories.Postgres;

namespace Repositories;

public interface IWebhookRepository
{

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
}
