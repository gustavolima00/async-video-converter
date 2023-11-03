using Xabe.FFmpeg;

namespace Repositories.Models;

public class MediaMetadata
{
    public IEnumerable<MediaStream> Streams { get; set; } = new List<MediaStream>();

    public string Path { get; set; } = "";

    public TimeSpan Duration { get; set; }

    public DateTime? CreationTime { get; set; }

    public long Size { get; set; }

    public IEnumerable<VideoStream> VideoStreams { get; set; } = new List<VideoStream>();

    public IEnumerable<AudioStream> AudioStreams { get; set; } = new List<AudioStream>();

    public IEnumerable<SubtitleStream> SubtitleStreams { get; set; } = new List<SubtitleStream>();

    public MediaMetadata() { }

    public MediaMetadata(IMediaInfo mediaInfo)
    {
        Streams = mediaInfo.Streams.Select(s => new MediaStream(s));
        Path = mediaInfo.Path;
        Duration = mediaInfo.Duration;
        CreationTime = mediaInfo.CreationTime;
        Size = mediaInfo.Size;
        VideoStreams = mediaInfo.VideoStreams.Select(s => new VideoStream(s));
        AudioStreams = mediaInfo.AudioStreams.Select(s => new AudioStream(s));
        SubtitleStreams = mediaInfo.SubtitleStreams.Select(s => new SubtitleStream(s));
    }
}

public class MediaStream
{
    public string Path { get; set; } = "";

    public int Index { get; set; }

    public string Codec { get; set; } = "";

    public StreamType StreamType { get; set; }

    public MediaStream() { }

    public MediaStream(IStream stream)
    {
        Path = stream.Path;
        Index = stream.Index;
        Codec = stream.Codec;
        StreamType = stream.StreamType;
    }
}

public class VideoStream
{
    public TimeSpan Duration { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public double Framerate { get; set; }

    public string Ratio { get; set; } = "";

    public long Bitrate { get; set; }

    public int? Default { get; set; }

    public int? Forced { get; set; }

    public string PixelFormat { get; set; } = "";

    public int? Rotation { get; set; }

    public string Path { get; set; } = "";

    public int Index { get; set; }

    public string Codec { get; set; } = "";

    public StreamType StreamType { get; set; }

    public VideoStream() { }

    public VideoStream(IVideoStream stream)
    {
        Duration = stream.Duration;
        Width = stream.Width;
        Height = stream.Height;
        Framerate = stream.Framerate;
        Ratio = stream.Ratio;
        Bitrate = stream.Bitrate;
        Default = stream.Default;
        Forced = stream.Forced;
        PixelFormat = stream.PixelFormat;
        Rotation = stream.Rotation;
        Path = stream.Path;
        Index = stream.Index;
        Codec = stream.Codec;
        StreamType = stream.StreamType;
    }
}

public class AudioStream
{
    public TimeSpan Duration { get; set; }

    public long Bitrate { get; set; }

    public int SampleRate { get; set; }

    public int Channels { get; set; }

    public string Language { get; set; } = "";

    public string Title { get; set; } = "";

    public int? Default { get; set; }

    public int? Forced { get; set; }

    public string Path { get; set; } = "";

    public int Index { get; set; }

    public string Codec { get; set; } = "";

    public StreamType StreamType { get; set; }

    public AudioStream() { }

    public AudioStream(IAudioStream stream)
    {
        Duration = stream.Duration;
        Bitrate = stream.Bitrate;
        SampleRate = stream.SampleRate;
        Channels = stream.Channels;
        Language = stream.Language;
        Title = stream.Title;
        Default = stream.Default;
        Forced = stream.Forced;
        Path = stream.Path;
        Index = stream.Index;
        Codec = stream.Codec;
        StreamType = stream.StreamType;
    }
}

public class SubtitleStream
{
    public string Language { get; set; } = "";

    public int? Default { get; set; }

    public int? Forced { get; set; }

    public string Title { get; set; } = "";

    public string Path { get; set; } = "";

    public int Index { get; set; }

    public string Codec { get; set; } = "";

    public StreamType StreamType { get; set; }

    public SubtitleStream() { }

    public SubtitleStream(ISubtitleStream stream)
    {
        Language = stream.Language;
        Default = stream.Default;
        Forced = stream.Forced;
        Title = stream.Title;
        Path = stream.Path;
        Index = stream.Index;
        Codec = stream.Codec;
        StreamType = stream.StreamType;
    }
}
