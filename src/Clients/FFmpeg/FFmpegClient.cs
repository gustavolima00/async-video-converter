using MediaToolkit;
using MediaToolkit.Model;

namespace Clients.FFmpeg;

public interface IFFmpegClient
{
    Metadata GetFileMetadata(string path);
}

public class FFmpegClient : IFFmpegClient
{
    private readonly string _ffmpegPath;

    public FFmpegClient(FFmpegClientConfiguration configuration)
    {
        _ffmpegPath = configuration.FFmpegPath;
    }

    public Metadata GetFileMetadata(string path)
    {
        var inputFile = new MediaFile { Filename = path };

        using var engine = new Engine(_ffmpegPath);
        engine.GetMetadata(inputFile);
        return inputFile.Metadata;
    }
}