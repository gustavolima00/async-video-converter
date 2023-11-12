using Xabe.FFmpeg;

namespace Services.Models;

public class SubtitleInfo
{
  public string Name { get; set; }
  public string Language { get; set; }
  public Stream Stream { get; set; }

  public SubtitleInfo(ISubtitleStream subtitleStream, Stream stream)
  {
    Name = subtitleStream.Title;
    Language = subtitleStream.Language;
    Stream = stream;
  }
}
