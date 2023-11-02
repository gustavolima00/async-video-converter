using Microsoft.Extensions.Configuration;

namespace Clients.FFmpeg;

public class FFmpegClientConfiguration
{
    public string DirectoryWithFFmpegAndFFprobe { get; set; } = "/usr/bin";
    public string FFmpegExeutableName { get; set; } = "ffmpeg";
    public string FFprobeExeutableName { get; set; } = "ffprobe";

    public static FFmpegClientConfiguration FromConfiguration(IConfigurationSection configurationSection)
    {
        var configuration = new FFmpegClientConfiguration();
        configuration.DirectoryWithFFmpegAndFFprobe =
            configurationSection.GetSection(nameof(DirectoryWithFFmpegAndFFprobe)).Value
            ?? configuration.DirectoryWithFFmpegAndFFprobe;

        configuration.FFmpegExeutableName =
            configurationSection.GetSection(nameof(FFmpegExeutableName)).Value
            ?? configuration.FFmpegExeutableName;

        configuration.FFprobeExeutableName =
            configurationSection.GetSection(nameof(FFprobeExeutableName)).Value
            ?? configuration.FFprobeExeutableName;

        return configuration;
    }
}