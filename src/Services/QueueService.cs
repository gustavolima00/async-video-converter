using Clients.RabbitMQ;
using Services.Configuration;
using Services.Models;

namespace Services;

public interface IQueueService
{
    void EnqueueFileToFillMetadata(FileToFillMetadata fileToFillMetadata);
    void EnqueueFileToConvert(int id);

    void EnqueueMessage<T>(string queueName, T message);
    IEnumerable<(string messageId, T message)> ReadMessages<T>(string queueName, int maxMessages = 10);
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
        using var connection = _rabbitMQClient.CreateConnection();
        _rabbitMQClient.SendMessage(connection, queueName, message);
    }

    public void EnqueueFileToFillMetadata(FileToFillMetadata fileToFillMetadata)
    {
        EnqueueMessage(_queuesConfiguration.FillMetadataQueueName, fileToFillMetadata);
    }

    public void EnqueueFileToConvert(int id)
    {
        EnqueueMessage(_queuesConfiguration.ConvertQueueName, id);
    }

    public IEnumerable<(string messageId, T message)> ReadMessages<T>(string queueName, int maxMessages = 10)
    {
        using var connection = _rabbitMQClient.CreateConnection();
        var messages = new List<(string messageId, T message)>();
        for (int i = 0; i < maxMessages; i++)
        {
            var message = _rabbitMQClient.ReadMessage<T>(connection, queueName);
            if (message is null)
            {
                break;
            }
            messages.Add((message.DeliveryTag, message.Payload));
        }
        return messages;
    }
}
