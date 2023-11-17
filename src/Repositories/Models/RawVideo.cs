using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Repositories.Models;

[Table("raw_videos")]
public class RawVideo
{
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";

    [Column("path")]
    public string Path { get; set; } = "";

    [Column("extract_subtitle_status", TypeName = "varchar(255)")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AsyncTaskStatus ExtractSubtitleStatus { get; set; } = AsyncTaskStatus.Pending;

    [Column("extract_tracks_status", TypeName = "varchar(255)")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AsyncTaskStatus ExtractTracksStatus { get; set; } = AsyncTaskStatus.Pending;

    [Column("user_uuid")]
    public Guid UserUuid { get; set; }
    public virtual ICollection<RawSubtitle> Subtitles { get; set; } = null!;

    public virtual ConvertedVideo ConvertedVideo { get; set; } = null!;
}
