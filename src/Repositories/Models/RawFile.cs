using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Repositories.Models;

[Table("raw_files")]
public class RawFile
{
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";

    [Column("path")]
    public string Path { get; set; } = "";

    [Column("conversion_status", TypeName = "varchar(255)")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ConversionStatus ConversionStatus { get; set; } = ConversionStatus.NotConverted;

    [Column("metadata")]
    public MediaMetadata? Metadata { get; set; }

    [Column("user_uuid")]
    public Guid UserUuid { get; set; }
}

public enum ConversionStatus
{
    NotConverted,
    Converting,
    Converted,
    Error
}
