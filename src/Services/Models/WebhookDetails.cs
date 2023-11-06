using System.Text.Json.Serialization;

namespace Services.Models;

public class WebHookDetails
{
  [JsonConverter(typeof(JsonStringEnumConverter))]

  public WebhookType WebhookType { get; set; }

  public Guid UserUuid { get; set; }
}

public enum WebhookType
{
  VideoConversionFinished,
  SubtitleConversionFinished,
}
