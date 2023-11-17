using Xabe.FFmpeg;

namespace Services.Models;

public class VideoTrackInfo
{
  public string Name { get; set; }
  public string Language { get; set; }
  public Stream Stream { get; set; }

  public VideoTrackInfo(IAudioStream audioMetadata, Stream stream)
  {
    Name = audioMetadata.Title;
    Language = audioMetadata.Language;
    Stream = stream;
  }
}
