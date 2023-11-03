using System.Text.Json;
using Npgsql;

namespace Repositories.Models;

public class RawFile
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public ConversionStatus ConversionStatus { get; set; } = ConversionStatus.NotConverted;
    public Metadata? Metadata { get; set; }

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
                       : JsonSerializer.Deserialize<Metadata>(reader.GetString(metadataOrdinal));

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

public class Metadata
{
    public TimeSpan Duration { get; set; }
    public long Size { get; set; }
    public IEnumerable<AudioStream> AudioStreams { get; set; } = new List<AudioStream>();
    public IEnumerable<SubtitleStream> SubtitleStreams { get; set; } = new List<SubtitleStream>();

}

public class AudioStream
{
    public TimeSpan Duration { get; set; }
    public long Bitrate { get; set; }
    public int SampleRate { get; set; }
    public int Channels { get; set; }
    public string Language { get; set; } = "";
    public string? Title { get; set; }
    public int? Default { get; set; }
    public int? Forced { get; set; }
}

public class SubtitleStream
{
    public string Language { get; set; } = "";
    public string? Title { get; set; }
    public int? Default { get; set; }
    public int? Forced { get; set; }
}