using System.Text.Json;
using Npgsql;


namespace Repositories.Models;

public class WebVideo
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string Link { get; set; } = "";

    public int RawFileId { get; set; }

    public MediaMetadata? Metadata { get; set; }

    public IEnumerable<WebVideoSubtitle> Subtitles { get; set; } = Enumerable.Empty<WebVideoSubtitle>();

    public string GetFormat()
    {
        return Name.Split('.').Last();
    }

    public static IEnumerable<string> FieldsNames()
    {
        yield return "web_videos.id as web_videos_id";
        yield return "web_videos.name as web_videos_name";
        yield return "web_videos.link as web_videos_link";
        yield return "web_videos.raw_file_id as web_videos_raw_file_id";
        yield return "web_videos.metadata as web_videos_metadata";
        yield return "web_videos.path as web_videos_path";
    }

    private static WebVideo? BuildWithoutSubtitlesFromReader(NpgsqlDataReader reader)
    {
        if (reader.IsDBNull(reader.GetOrdinal("web_videos_id")))
        {
            return null;
        }

        int idOrdinal = reader.GetOrdinal("web_videos_id");
        int nameOrdinal = reader.GetOrdinal("web_videos_name");
        int linkOrdinal = reader.GetOrdinal("web_videos_link");
        int rawFileIdOrdinal = reader.GetOrdinal("web_videos_raw_file_id");
        int metadataOrdinal = reader.GetOrdinal("web_videos_metadata");
        int pathOrdinal = reader.GetOrdinal("web_videos_path");
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
                : JsonSerializer.Deserialize<MediaMetadata>(reader.GetString(metadataOrdinal));
        if(pathOrdinal >= 0)
            webVideo.Path = reader.GetString(pathOrdinal);

        return webVideo;
    }

    public static async Task<WebVideo?> BuildFromReader(NpgsqlDataReader reader, CancellationToken cancellationToken = default)
    {
        WebVideo? webVideo = null;
        List<WebVideoSubtitle> subtitles = new();

        while (await reader.ReadAsync(cancellationToken))
        {
            webVideo ??= BuildWithoutSubtitlesFromReader(reader);
            var subtitle = WebVideoSubtitle.BuildFromReader(reader);
            if (subtitle is not null)
            {
                subtitles.Add(subtitle);
            }
        }
        if (webVideo is not null)
        {
            webVideo.Subtitles = subtitles;
        }
        return webVideo;
    }

    public static async Task<IEnumerable<WebVideo>> BuildMultipleFromReader(NpgsqlDataReader reader, CancellationToken cancellationToken = default)
    {
        List<WebVideo> webVideos = new();
        WebVideo? currentWebVideo = null;

        while (await reader.ReadAsync(cancellationToken))
        {
            Console.WriteLine(reader);
            var webVideo = BuildWithoutSubtitlesFromReader(reader);
            var subtitle = WebVideoSubtitle.BuildFromReader(reader);
            Console.WriteLine($"Web video: {webVideo}");
            Console.WriteLine($"subtitle: {subtitle}");

            if (webVideo is null)
            {
                break;
            }
            if (currentWebVideo?.Id != webVideo.Id)
            {
                if (currentWebVideo is not null)
                {
                    webVideos.Add(currentWebVideo);
                }
                else
                {
                    currentWebVideo = webVideo;
                }
            }

            if (subtitle is not null)
            {
                _ = currentWebVideo.Subtitles.Append(subtitle);
            }
        }
        if (currentWebVideo is not null)
        {
            webVideos.Add(currentWebVideo);
        }

        return webVideos;
    }
}


public class WebVideoSubtitle
{
    public int Id { get; set; }
    public int WebVideoId { get; set; }
    public string Path { get; set; } = "";
    public string Language { get; set; } = "";
    public string Link { get; set; } = "";
    public MediaMetadata? Metadata { get; set; }

    public static IEnumerable<string> FieldsNames()
    {
        yield return "web_video_subtitles.id as web_video_subtitles_id";
        yield return "web_video_subtitles.web_video_id as web_video_subtitles_web_video_id";
        yield return "web_video_subtitles.language as web_video_subtitles_language";
        yield return "web_video_subtitles.link as web_video_subtitles_link";
        yield return "web_video_subtitles.metadata as web_video_subtitles_metadata";
        yield return "web_video_subtitles.path as web_video_subtitles_path";
    }

    public static WebVideoSubtitle? BuildFromReader(NpgsqlDataReader reader)
    {
        if (reader.IsDBNull(reader.GetOrdinal("web_video_subtitles_id")))
        {
            return null;
        }

        int idOrdinal = reader.GetOrdinal("web_video_subtitles_id");
        int webVideoIdOrdinal = reader.GetOrdinal("web_video_subtitles_web_video_id");
        int languageOrdinal = reader.GetOrdinal("web_video_subtitles_language");
        int linkOrdinal = reader.GetOrdinal("web_video_subtitles_link");
        int metadataOrdinal = reader.GetOrdinal("web_video_subtitles_metadata");
        int pathOrdinal = reader.GetOrdinal("web_video_subtitles_path");
        var webVideoSubtitle = new WebVideoSubtitle();
        if (idOrdinal >= 0)
            webVideoSubtitle.Id = reader.GetInt32(idOrdinal);

        if (webVideoIdOrdinal >= 0)
            webVideoSubtitle.WebVideoId = reader.GetInt32(webVideoIdOrdinal);

        if (languageOrdinal >= 0)
            webVideoSubtitle.Language = reader.GetString(languageOrdinal);

        if (linkOrdinal >= 0)
            webVideoSubtitle.Link = reader.GetString(linkOrdinal);

        if (metadataOrdinal >= 0)
            webVideoSubtitle.Metadata = reader.IsDBNull(metadataOrdinal)
                ? null
                : JsonSerializer.Deserialize<MediaMetadata>(reader.GetString(metadataOrdinal));
        if(pathOrdinal >= 0)
            webVideoSubtitle.Path = reader.GetString(pathOrdinal);


        return webVideoSubtitle;
    }
}
