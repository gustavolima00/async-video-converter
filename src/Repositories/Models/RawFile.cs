namespace Repositories.Models;

public class RawFile
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public Metadata? Metadata { get; set; } = new Metadata();

    public string GetFormat()
    {
        return Name.Split('.').Last();
    }
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