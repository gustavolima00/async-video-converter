using System.Text;
using Xabe.FFmpeg;

namespace Clients.FFmpeg;

public interface IFFmpegClient
{
    Task<IMediaInfo> GetFileMetadata(Stream stream, string fileExtension, CancellationToken cancellationToken = default);
    Task<Stream> ConvertToMp4(Stream stream, string fileExtension, CancellationToken cancellationToken = default);
    Task<Stream> ConvertSrtToVtt(Stream srtStream, CancellationToken cancellationToken = default);
    Task<List<(ISubtitleStream metadata, Stream stream)>> ExtractSubtitles(Stream videoStream, CancellationToken cancellationToken = default);
}

public class FFmpegClient : IFFmpegClient
{

    public FFmpegClient(FFmpegClientConfiguration configuration)
    {
        Xabe.FFmpeg.FFmpeg.SetExecutablesPath(
            configuration.DirectoryWithFFmpegAndFFprobe,
            configuration.FFmpegExeutableName,
            configuration.FFprobeExeutableName);
    }

    private async static Task<string> SaveStreamIntoTempFile(Stream stream, string fileExtension, CancellationToken cancellationToken = default)
    {
        string tempFilePath = Path.GetTempFileName();
        tempFilePath = Path.ChangeExtension(tempFilePath, fileExtension);
        Console.WriteLine($"Temp file path: {tempFilePath}");
        using var fileStream = File.Create(tempFilePath);
        Console.WriteLine($"File created: {tempFilePath}");
        stream.Seek(0, SeekOrigin.Begin);
        await stream.CopyToAsync(fileStream, cancellationToken);
        return tempFilePath;
    }

    static async Task<IMediaInfo> GetFileMetadata(string path, CancellationToken cancellationToken = default)
    {
        return await Xabe.FFmpeg.FFmpeg.GetMediaInfo(path, cancellationToken);
    }

    public async Task<IMediaInfo> GetFileMetadata(Stream stream, string fileExtension, CancellationToken cancellationToken = default)
    {
        string? videoFilePath = null;
        try
        {
            videoFilePath = await SaveStreamIntoTempFile(stream, fileExtension, cancellationToken);
            return await GetFileMetadata(videoFilePath, cancellationToken);
        }
        finally
        {
            if (videoFilePath is not null)
            {
                File.Delete(videoFilePath);
            }
        }
    }

    public static async Task ConvertToMp4(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        var conversion = await Xabe.FFmpeg.FFmpeg.Conversions.FromSnippet.Convert(inputPath, outputPath, true);
        await conversion.Start(cancellationToken);
    }

    public async Task<Stream> ConvertToMp4(Stream stream, string fileExtension, CancellationToken cancellationToken = default)
    {
        string? videoFilePath = null;
        string? outputFilePath = null;
        try
        {
            videoFilePath = await SaveStreamIntoTempFile(stream, fileExtension, cancellationToken);
            outputFilePath = Path.ChangeExtension(videoFilePath, "mp4");
            await ConvertToMp4(videoFilePath, outputFilePath, cancellationToken);
            return File.OpenRead(outputFilePath);
        }
        finally
        {
            if (videoFilePath is not null)
            {
                File.Delete(videoFilePath);
            }
            if (outputFilePath is not null)
            {
                File.Delete(outputFilePath);
            }
        }
    }

    public async Task<Stream> ConvertSrtToVtt(Stream srtStream, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(srtStream);
        using var writer = new StringWriter();

        await writer.WriteLineAsync("WEBVTT");
        await writer.WriteLineAsync();

        string? line;
        while ((line = await reader.ReadLineAsync()) is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (int.TryParse(line, out _))
            {
                continue;
            }

            line = line.Replace(',', '.');

            await writer.WriteLineAsync(line);
        }

        return new MemoryStream(Encoding.UTF8.GetBytes(writer.ToString()));
    }

    public async Task<List<(ISubtitleStream, Stream)>> ExtractSubtitles(Stream videoStream, CancellationToken cancellationToken = default)
    {
        string? videoPath = null;
        List<string> subtitlesPaths = new();
        try
        {
            videoPath = await SaveStreamIntoTempFile(videoStream, "mp4", cancellationToken);
            var mediaInfo = await Xabe.FFmpeg.FFmpeg.GetMediaInfo(videoPath);
            var subtitleStreams = mediaInfo.SubtitleStreams;
            List<(ISubtitleStream metadata, Stream stream)> subtitles = new();

            List<string> subtitleFiles = new();

            foreach (var stream in subtitleStreams)
            {
                string tempFilePath = Path.GetTempFileName();
                string subtitlePath = Path.ChangeExtension(tempFilePath, "srt");
                var conversion = Xabe.FFmpeg.FFmpeg.Conversions.New()
                                    .AddStream(stream)
                                    .SetOutput(subtitlePath);

                await conversion.Start(cancellationToken);
                subtitleFiles.Add(subtitlePath);
                subtitles.Add((stream, File.OpenRead(subtitlePath)));
            }

            return subtitles;
        }
        finally
        {
            if (videoPath is not null)
            {
                File.Delete(videoPath);
            }
            subtitlesPaths.ForEach(File.Delete);
        }
    }
}
