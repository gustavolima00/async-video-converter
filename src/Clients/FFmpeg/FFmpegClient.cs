using System.Text;
using Xabe.FFmpeg;

namespace Clients.FFmpeg;

public interface IFFmpegClient
{
    Task<Stream> ConvertToMp4(Stream stream, string fileExtension, CancellationToken cancellationToken = default);
    Task<Stream> ConvertSrtToVtt(Stream srtStream, CancellationToken cancellationToken = default);
    Task<List<(ISubtitleStream metadata, Stream stream)>> ExtractSubtitles(Stream videoStream, string videoExtension, CancellationToken cancellationToken = default);
    Task<List<(IAudioStream metadata, Stream stream)>> ExtractVideoTracks(Stream videoStream, string videoExtension, CancellationToken cancellationToken = default);
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

    private static async Task<IMediaInfo> GetFileMetadata(string path, CancellationToken cancellationToken = default)
    {
        return await Xabe.FFmpeg.FFmpeg.GetMediaInfo(path, cancellationToken);
    }

    private static IConversion NewConversion()
    {
        return Xabe.FFmpeg.FFmpeg.Conversions.New();
    }

    public async Task<List<(IAudioStream, Stream)>> ExtractVideoTracks(Stream videoStream, string videoExtension, CancellationToken cancellationToken = default)
    {
        string? videoPath = null;
        List<string> tracksPaths = new();
        try
        {
            videoPath = await SaveStreamIntoTempFile(videoStream, videoExtension, cancellationToken);
            var mediaInfo = await GetFileMetadata(videoPath, cancellationToken);

            List<(IAudioStream metadata, Stream stream)> videoTracks = new();

            var tasks = mediaInfo.AudioStreams.Select(async stream =>
            {
                string tempFilePath = Path.GetTempFileName();
                tempFilePath = Path.ChangeExtension(tempFilePath, "mp4");

                Console.WriteLine($"Converting for track: {stream.Index}");
                var conversion = NewConversion()
                    .AddStream(mediaInfo.VideoStreams)
                    .AddStream(stream)
                    .SetOutput(tempFilePath);

                conversion.OnProgress += (sender, args) =>
                {
                    Console.Write($"\rProgresso da faixa {stream.Index}: {args.Percent}%");
                };

                await conversion.Start(cancellationToken);

                Console.WriteLine($"Track converted: {stream.Index}");

                tracksPaths.Add(tempFilePath);
                videoTracks.Add((stream, File.OpenRead(tempFilePath)));
            });

            await Task.WhenAll(tasks);

            return videoTracks;
        }
        finally
        {
            if (videoPath is not null)
            {
                File.Delete(videoPath);
            }
            tracksPaths.ForEach(File.Delete);
        }
    }

    public static async Task ConvertToMp4(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        var conversion = await Xabe.FFmpeg.FFmpeg.Conversions.FromSnippet.Convert(inputPath, outputPath, true);
        await conversion.Start(cancellationToken);
    }

    public async Task<Stream> ConvertToMp4(Stream stream, string fileExtension, CancellationToken cancellationToken = default)
    {
        if (fileExtension == "mp4")
        {
            return stream;
        }

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

    public async Task<List<(ISubtitleStream, Stream)>> ExtractSubtitles(Stream videoStream, string videoExtension, CancellationToken cancellationToken = default)
    {
        string? videoPath = null;
        List<string> subtitlesPaths = new();
        try
        {
            videoPath = await SaveStreamIntoTempFile(videoStream, videoExtension, cancellationToken);
            var mediaInfo = await GetFileMetadata(videoPath, cancellationToken);
            var subtitleStreams = mediaInfo.SubtitleStreams;
            List<(ISubtitleStream metadata, Stream stream)> subtitles = new();

            List<string> subtitleFiles = new();

            var tasks = mediaInfo.SubtitleStreams.Select(async stream =>
            {
                string tempFilePath = Path.GetTempFileName();
                string subtitlePath = Path.ChangeExtension(tempFilePath, "vtt");
                var conversion = NewConversion()
                                    .AddStream(stream)
                                    .SetOutput(subtitlePath);

                await conversion.Start(cancellationToken);
                subtitleFiles.Add(subtitlePath);
                subtitles.Add((stream, File.OpenRead(subtitlePath)));
            });

            await Task.WhenAll(tasks);

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
