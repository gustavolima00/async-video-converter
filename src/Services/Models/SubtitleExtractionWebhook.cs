namespace Services.Models;

public class SubtitleExtractionWebhook
{
  public Guid UserUuid { get; set; }
  public Guid RawVideoUuid { get; set; }
}
