using System.Text.Json.Serialization;

namespace Services.Models;

public enum WebhookEvent
{
  VideoTracksExtracted,
  SubtitleTracksExtracted,
  VideoTrackExtractionFailed,
  SubtitleTrackExtractionFailed,
}

public class WebHookDetails
{
  [JsonConverter(typeof(JsonStringEnumConverter))]

  public WebhookEvent Event { get; set; }

  public Guid UserUuid { get; set; }
  public string? Error { get; set; }
  public string Payload { get; set; } = "";
}


