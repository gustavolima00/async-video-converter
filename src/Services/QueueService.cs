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
    private readonly IConnection _rabbitMQConnection;
    public QueueService(IRabbitMQClient rabbitMQClient)
    {
        _rabbitMQClient = rabbitMQClient;
        _rabbitMQConnection = _rabbitMQClient.CreateConnection();
    }
    public void SendMessage<T>(string queueName, T message)
    {
        _rabbitMQClient.SendMessage(_rabbitMQConnection, queueName, message);
    }
    public (string messageId, T message)? ReadMessage<T>(string queueName)
    {
        var message = _rabbitMQClient.ReadMessage<T>(_rabbitMQConnection, queueName);
        if (message is null)
        {
            return null;
        }
        return (message.DeliveryTag, message.Payload);
    }

    public IEnumerable<(string messageId, T message)> ReadMessages<T>(string queueName, int maxMessages = 10)
    {
        var messages = new List<(string messageId, T message)>();
        for (int i = 0; i < maxMessages; i++)
        {
            var message = ReadMessage<T>(queueName);
            if (message is null)
            {
                break;
            }
            messages.Add((message.Value.messageId, message.Value.message));
        }
        return messages;
    }

    public void DeleteMessage(string queueName, string messageId)
    {
        _rabbitMQClient.DeleteMessage(_rabbitMQConnection, queueName, messageId);
    }
}