using Microsoft.Extensions.Configuration;

namespace Clients.FFmpeg;

public class FFmpegClientConfiguration
{
    public string FFmpegPath { get; set; } = "/usr/bin/ffmpeg";

    public static FFmpegClientConfiguration FromConfiguration(IConfigurationSection configurationSection)
    {
        return new FFmpegClientConfiguration
        {
            FFmpegPath = configurationSection.GetSection(nameof(FFmpegPath)).Value ?? "/usr/bin/ffmpeg"
        };
    }
}