using Clients.BlobStorage;
using Clients.FFmpeg;
using Clients.RabbitMQ;

namespace Clients;

public class ClientsConfiguration
{
    public BlobStorageClientConfiguration BlobStorageClientConfiguration { get; set; } = new BlobStorageClientConfiguration();
    public FFmpegClientConfiguration FFmpegClientConfiguration { get; set; } = new FFmpegClientConfiguration();
    public RabbitMQClientConfiguration RabbitMQClientConfiguration { get; set; } = new RabbitMQClientConfiguration();
}