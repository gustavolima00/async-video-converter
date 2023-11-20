using System.Text.Json;
using Clients.RabbitMQ;
using Services.Configuration;
using Services.Models;

namespace Services;

public interface IQueueService
{
    void EnqueueVideoToExtractTracks(VideoToExtractVideoTracks videoToExtractTracks);
    void EnqueueVideoToExtractSubtitles(VideoToExtractSubtitles videoToExtractSubtitles);
    void EnqueueWebhook<T>(WebHookDetails<T> webhookDetails, string url);
    void EnqueueMessage<T>(string queueName, T message);
    IEnumerable<(ulong messageId, T message)> ReadMessages<T>(string queueName, int maxMessages = 10);
    void DeleteMessage(ulong messageId);
}

public class QueueService : IQueueService
{
    private readonly IRabbitMQClient _rabbitMQClient;
    private readonly QueuesConfiguration _queuesConfiguration;

    public QueueService(IRabbitMQClient rabbitMQClient, QueuesConfiguration queuesConfiguration)
    {
        _rabbitMQClient = rabbitMQClient;
        _queuesConfiguration = queuesConfiguration;
    }
    public void EnqueueMessage<T>(string queueName, T message)
    {
        _rabbitMQClient.SendMessage(queueName, message);
    }

    public void EnqueueVideoToExtractTracks(VideoToExtractVideoTracks videoToExtractTracks)
    {
        EnqueueMessage(_queuesConfiguration.ExtractVideoTracksQueueName, videoToExtractTracks);
    }

    public void EnqueueVideoToExtractSubtitles(VideoToExtractSubtitles videoToExtractSubtitles)
    {
        EnqueueMessage(_queuesConfiguration.ExtractSubtitlesQueueName, videoToExtractSubtitles);
    }

    public void EnqueueWebhook<T>(WebHookDetails<T> webhookDetails, string url)
    {
        var content = JsonSerializer.Serialize(webhookDetails);
        EnqueueMessage(_queuesConfiguration.WebhookQueueName, new WebHookToEnqueue {
            SerializedData = content,
            Url = url
        });
    }

    public IEnumerable<(ulong messageId, T message)> ReadMessages<T>(string queueName, int maxMessages = 10)
    {
        var messages = new List<(ulong messageId, T message)>();
        for (int i = 0; i < maxMessages; i++)
        {
            var message = _rabbitMQClient.ReadMessage<T>(queueName);
            if (message is null)
            {
                break;
            }
            messages.Add((message.DeliveryTag, message.Payload));
        }
        return messages;
    }

    public void DeleteMessage(ulong messageId)
    {
        _rabbitMQClient.AckMessage(messageId);
    }
}
