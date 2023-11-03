using System.Text.Json;
using System.Text.Json.Serialization;
using Npgsql;

namespace Repositories.Models;

public class RawFile
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ConversionStatus ConversionStatus { get; set; } = ConversionStatus.NotConverted;
    public MediaMetadata? Metadata { get; set; }

    public string GetFormat()
    {
        return Name.Split('.').Last();
    }

    public static RawFile BuildFromReader(NpgsqlDataReader reader)
    {
        int idOrdinal = reader.GetOrdinal("id");
        int nameOrdinal = reader.GetOrdinal("name");
        int pathOrdinal = reader.GetOrdinal("path");
        int conversionStatusOrdinal = reader.GetOrdinal("conversion_status");
        int metadataOrdinal = reader.GetOrdinal("metadata");
        var rawFile = new RawFile();
        if (idOrdinal >= 0)
            rawFile.Id = reader.GetInt32(idOrdinal);

        if (nameOrdinal >= 0)
            rawFile.Name = reader.GetString(nameOrdinal);

        if (pathOrdinal >= 0)
            rawFile.Path = reader.GetString(pathOrdinal);

        if (conversionStatusOrdinal >= 0)
        {
            var statusString = reader.GetString(conversionStatusOrdinal);
            if (Enum.TryParse(statusString, out ConversionStatus status))
            {
                rawFile.ConversionStatus = status;
            }
            else
            {
                throw new Exception($"Unknown conversion status: {statusString}");
            }
        }


        if (metadataOrdinal >= 0)
            rawFile.Metadata = reader.IsDBNull(metadataOrdinal)
                       ? null
                       : JsonSerializer.Deserialize<MediaMetadata>(reader.GetString(metadataOrdinal));

        return rawFile;
    }
}

public enum ConversionStatus
{
    NotConverted,
    Converting,
    Converted,
    Error
}