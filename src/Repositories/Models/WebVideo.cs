using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Npgsql;


namespace Repositories.Models;

[Table("web_videos")]
public class WebVideo
{
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";

    [Column("path")]
    public string Path { get; set; } = "";

    [Column("link")]
    public string Link { get; set; } = "";

    [Column("raw_file_id")]

    public int RawFileId { get; set; }

    [Column("metadata")]

    public MediaMetadata? Metadata { get; set; }

    public ICollection<WebVideoSubtitle> Subtitles { get; set; } = new List<WebVideoSubtitle>();
}


public class WebVideoSubtitle
{

    [Column("id")]
    public int Id { get; set; }

    [Column("web_video_id")]
    public int WebVideoId { get; set; }

    [Column("path")]
    public string Path { get; set; } = "";

    [Column("language")]
    public string Language { get; set; } = "";

    [Column("link")]
    public string Link { get; set; } = "";

    [Column("metadata")]
    public MediaMetadata? Metadata { get; set; }
}
