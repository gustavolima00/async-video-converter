using System.ComponentModel.DataAnnotations.Schema;

namespace Repositories.Models;

[Table("raw_subtitles")]
public class RawSubtitle
{
    [Column("id")]
    public int Id { get; set; }

    [Column("language")]
    public string Language { get; set; } = "";

    [Column("path")]
    public string Path { get; set; } = "";

    [Column("raw_video_id")]
    public int RawVideoId { get; set; }
}

