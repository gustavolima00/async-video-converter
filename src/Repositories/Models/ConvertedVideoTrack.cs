using System.ComponentModel.DataAnnotations.Schema;


namespace Repositories.Models;

[Table("converted_video_tracks")]
public class ConvertedVideoTrack
{
    [Column("id")]
    public int Id { get; set; }

    [Column("converted_video_id")]
    public int ConvertedVideoId { get; set; }

    [Column("path")]
    public string Path { get; set; } = "";

    [Column("language")]
    public string Language { get; set; } = "und";

    [Column("link")]
    public string Link { get; set; } = "";
}
