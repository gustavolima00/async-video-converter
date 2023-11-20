namespace Services.Models;

public class TrackDetails
{
  public string Path { get; set; } = "";
  public string Language { get; set; } = "und";
}

public class TrackExtranctionWebhook
{
  public Guid UserUuid { get; set; }
  public Guid RawVideoUuid { get; set; }

  public IEnumerable<TrackDetails> VideoTracks { get; set; } = new List<TrackDetails>();
}
