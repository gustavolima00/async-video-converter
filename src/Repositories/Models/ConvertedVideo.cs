using System.ComponentModel.DataAnnotations.Schema;


namespace Repositories.Models;

[Table("converted_videos")]
public class ConvertedVideo
{
    [Column("id")]
    public int Id { get; set; }

    [Column("raw_video_id")]

    public int RawVideoId { get; set; }

    public ICollection<ConvertedSubtitle> Subtitles { get; set; } = null!;

    public ICollection<ConvertedVideoTrack> Streams { get; set; } = null!;
}
