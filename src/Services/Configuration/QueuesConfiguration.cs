namespace Services.Configuration;

public class QueuesConfiguration
{
    public string FillMetadataQueueName { get; set; } = "fill_metadata";
    public string ConvertQueueName { get; set; } = "convert";
}