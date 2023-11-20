using System.Text.Json.Serialization;

namespace Services.Models;

public enum WebhookEvent
{
  VideoTracksExtracted,
  SubtitleTracksExtracted,
  VideoTrackExtractionFailed,
  SubtitleTrackExtractionFailed,
}

public class WebHookDetails<T>
{
  [JsonConverter(typeof(JsonStringEnumConverter))]

  public WebhookEvent Event { get; set; }

  public Guid UserUuid { get; set; }
  public string? Error { get; set; }
  public T? Payload { get; set; } = default;
}

public class WebHookToEnqueue
{
  public string SerializedData { get; set; } = "";
  public string Url { get; set; } = "";
}


