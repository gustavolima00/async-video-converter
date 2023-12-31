using System.ComponentModel.DataAnnotations.Schema;

namespace Repositories.Models;

[Table("webhook_users")]
public class WebhookUser
{
    [Column("id")]
    public int Id { get; set; }
    [Column("uuid")]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    [Column("webhook_url")]
    public string WebhookUrl { get; set; } = "";
    [Column("events")]
    public List<string> Events { get; set; } = new List<string>();
}
