using Clients.RabbitMQ;

namespace Services;

public interface IQueueService
{
    void SendMessage<T>(string queueName, T message);
    T? ReadMessage<T>(string queueName);
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
        _rabbitMQClient.SendMessage(queueName, message);
    }
    public T? ReadMessage<T>(string queueName)
    {
        return _rabbitMQClient.ReadMessage<T>(queueName);
    }
}