using Xabe.FFmpeg;

namespace Repositories.Models;

public class MediaMetadata : IMediaInfo
{
    public IEnumerable<IStream> Streams { get; } = new List<IStream>();

    public string Path { get; } = "";

    public TimeSpan Duration { get; }

    public DateTime? CreationTime { get; }

    public long Size { get; }

    public IEnumerable<IVideoStream> VideoStreams { get; } = new List<IVideoStream>();

    public IEnumerable<IAudioStream> AudioStreams { get; } = new List<IAudioStream>();

    public IEnumerable<ISubtitleStream> SubtitleStreams { get; } = new List<ISubtitleStream>();
}