using Clients.RabbitMQ;
using RabbitMQ.Client;

namespace Services;

public interface IQueueService
{
    void SendMessage<T>(string queueName, T message);
    (string messageId, T message)? ReadMessage<T>(string queueName);

    IEnumerable<(string messageId, T message)> ReadMessages<T>(string queueName, int maxMessages = 10);
    void DeleteMessage(string queueName, string messageId);
}

public class QueueService : IQueueService
{
    private readonly IRabbitMQClient _rabbitMQClient;
    public QueueService(IRabbitMQClient rabbitMQClient)
    {
        _rabbitMQClient = rabbitMQClient;
    }
    public void SendMessage<T>(string queueName, T message)
    {
        using var connection = _rabbitMQClient.CreateConnection();
        _rabbitMQClient.SendMessage(connection, queueName, message);
    }
    public (string messageId, T message)? ReadMessage<T>(string queueName)
    {
        using var connection = _rabbitMQClient.CreateConnection();
        var message = _rabbitMQClient.ReadMessage<T>(connection, queueName);
        if (message is null)
        {
            return null;
        }
        return (message.DeliveryTag, message.Payload);
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

    public void DeleteMessage(string queueName, string messageId)
    {
        using var connection = _rabbitMQClient.CreateConnection();
        _rabbitMQClient.DeleteMessage(connection, queueName, messageId);
    }
}