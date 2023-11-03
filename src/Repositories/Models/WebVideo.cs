using System.Text.Json;
using System.Text.Json.Serialization;
using Npgsql;
using Xabe.FFmpeg;


namespace Repositories.Models;

public class WebVideo
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Link { get; set; } = "";

    public int RawFileId { get; set; }

    public IMediaInfo? Metadata { get; set; }

    public string GetFormat()
    {
        return Name.Split('.').Last();
    }

    public static WebVideo BuildFromReader(NpgsqlDataReader reader)
    {
        int idOrdinal = reader.GetOrdinal("id");
        int nameOrdinal = reader.GetOrdinal("name");
        int linkOrdinal = reader.GetOrdinal("link");
        int rawFileIdOrdinal = reader.GetOrdinal("raw_file_id");
        int metadataOrdinal = reader.GetOrdinal("metadata");
        var webVideo = new WebVideo();
        if (idOrdinal >= 0)
            webVideo.Id = reader.GetInt32(idOrdinal);

        if (nameOrdinal >= 0)
            webVideo.Name = reader.GetString(nameOrdinal);

        if (linkOrdinal >= 0)
            webVideo.Link = reader.GetString(linkOrdinal);

        if (rawFileIdOrdinal >= 0)
            webVideo.RawFileId = reader.GetInt32(rawFileIdOrdinal);

        if (metadataOrdinal >= 0)
            webVideo.Metadata = reader.IsDBNull(metadataOrdinal)
                ? null
                : JsonSerializer.Deserialize<IMediaInfo>(reader.GetString(metadataOrdinal));

        return webVideo;
    }
}