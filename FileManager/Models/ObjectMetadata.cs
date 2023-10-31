namespace FileManager.Models;
using System.Text.Json.Serialization;

public class ObjectMetadata
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("size")]
    public long Size { get; set; }
    [JsonPropertyName("last_modified")]
    public DateTime LastModified { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("path")]
    public string Path { get; set; }
}