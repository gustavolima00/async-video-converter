namespace Services.Configuration;

public class QueuesConfiguration
{
    public string ExtractVideoTracksQueueName { get; set; } = "extract_video_tracks";
    public string WebhookQueueName { get; set; } = "webhook";
    public string ExtractSubtitlesQueueName { get; set; } = "extract_subtitles";
}
