using Xabe.FFmpeg;

namespace Clients.FFmpeg;

public interface IFFmpegClient
{
    Task<IMediaInfo> GetFileMetadata(Stream stream, string fileExtension, CancellationToken cancellationToken = default);
    Task<Stream> ConvertToMp4(Stream stream, string fileExtension, CancellationToken cancellationToken = default);
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
        var conversion = await Xabe.FFmpeg.FFmpeg.Conversions.FromSnippet.Convert(inputPath, outputPath);
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
}
