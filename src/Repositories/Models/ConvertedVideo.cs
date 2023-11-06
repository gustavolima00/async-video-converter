using System.ComponentModel.DataAnnotations.Schema;


namespace Repositories.Models;

[Table("web_videos")]
public class ConvertedVideo
{
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";

    [Column("path")]
    public string Path { get; set; } = "";

    [Column("link")]
    public string Link { get; set; } = "";

    [Column("raw_video_id")]

    public int RawVideoId { get; set; }

    [Column("metadata")]

    public MediaMetadata? Metadata { get; set; }

    public ICollection<ConvertedSubtitle> Subtitles { get; set; } = new List<ConvertedSubtitle>();
}

public class ConvertedSubtitle
{

    [Column("id")]
    public int Id { get; set; }

    [Column("converted_video_id")]
    public int ConvertedVideoId { get; set; }

    [Column("path")]
    public string Path { get; set; } = "";

    [Column("language")]
    public string Language { get; set; } = "";

    [Column("link")]
    public string Link { get; set; } = "";

    [Column("metadata")]
    public MediaMetadata? Metadata { get; set; }
}
