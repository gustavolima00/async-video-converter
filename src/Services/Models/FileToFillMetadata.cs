using System.Text.Json.Serialization;

namespace Services.Models;

public class FileToFillMetadata
{
  public int Id { get; set; }
  [JsonConverter(typeof(JsonStringEnumConverter))]

  public FileType FileType { get; set; }
}

public enum FileType
{
  RawVideo,
  RawSubtitle,
  ConvertedVideo,
  Subtitle
}
