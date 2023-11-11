using System.Text.Json.Serialization;

namespace Services.Models;

public class FileToConvert
{
  public int Id { get; set; }
  [JsonConverter(typeof(JsonStringEnumConverter))]

  public FileType FileType { get; set; }
}
