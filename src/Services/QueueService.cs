using Clients.RabbitMQ;
using Services.Configuration;
using Services.Models;

namespace Services;

public interface IQueueService
{
    void EnqueueFileToFillMetadata(FileToFillMetadata fileToFillMetadata);
    void EnqueueFileToConvert(int id);
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

    public void EnqueueFileToFillMetadata(FileToFillMetadata fileToFillMetadata)
    {
        EnqueueMessage(_queuesConfiguration.FillMetadataQueueName, fileToFillMetadata);
    }

    public void EnqueueFileToConvert(int id)
    {
        EnqueueMessage(_queuesConfiguration.ConvertQueueName, id);
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
