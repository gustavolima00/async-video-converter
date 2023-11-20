namespace Services.Models;

public class VideoToExtractVideoTracks
{
  public int RawVideoId { get; set; }
  public Guid UserUuid { get; set; }
  public Guid RawVideoUuid { get; set; }
}
