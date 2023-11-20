namespace Services.Models;

public class SubtitleDetails
{
  public string Path { get; set; } = "";
  public string Language { get; set; } = "und";
}

public class SubtitleExtractionWebhook
{
  public Guid UserUuid { get; set; }
  public Guid RawVideoUuid { get; set; }

  public IEnumerable<SubtitleDetails> Subtitles { get; set; } = new List<SubtitleDetails>();
}
