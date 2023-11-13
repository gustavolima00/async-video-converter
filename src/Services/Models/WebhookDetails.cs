using System.Text.Json.Serialization;

namespace Services.Models;

public enum WebhookEvent
{
  VideoConversionFinished,
  SubtitleConversionFinished,
}

public class WebHookDetails
{
  [JsonConverter(typeof(JsonStringEnumConverter))]

  public WebhookEvent Event { get; set; }

  public Guid UserUuid { get; set; }
}


