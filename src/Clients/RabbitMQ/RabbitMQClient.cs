using System.Text;
using System.Text.Json;
using Clients.RabbitMQ.Models;
using RabbitMQ.Client;

namespace Clients.RabbitMQ;

public interface IRabbitMQClient
{
    IConnection CreateConnection();
    void SendMessage<T>(IConnection connection, string queueName, T message);
    RabbitMQMessage<T>? ReadMessage<T>(IConnection connection, string queueName);
    void DeleteMessage(IConnection connection, string queueName, string deliveryTag);
}

public class RabbitMQClient : IRabbitMQClient
{
    private readonly RabbitMQClientConfiguration _configuration;
    public RabbitMQClient(RabbitMQClientConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IConnection CreateConnection()
    {
        var factory = new ConnectionFactory() { HostName = _configuration.Hostname };
        return factory.CreateConnection();
    }

    public static void SendMessageAsString(IConnection connection, string queueName, string message)
    {
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: queueName,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "",
                             routingKey: queueName,
                             basicProperties: null,
                             body: body);
    }

    public void SendMessage<T>(IConnection connection, string queueName, T message)
    {
        var messageAsString = JsonSerializer.Serialize(message);
        SendMessageAsString(connection, queueName, messageAsString);

    }

    public static RabbitMQMessage<string>? ReadMessageAsString(IConnection connection, string queueName)
    {
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: queueName,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var data = channel.BasicGet(queueName, false);
        if (data == null)
        {
            return null;
        }
        var message = Encoding.UTF8.GetString(data.Body.ToArray());
        var deliveryTag = data.DeliveryTag;
        return new RabbitMQMessage<string>(deliveryTag.ToString(), message);
    }

    public RabbitMQMessage<T>? ReadMessage<T>(IConnection connection, string queueName)
    {
        var messageAsString = ReadMessageAsString(connection, queueName);
        if (messageAsString == null)
        {
            return null;
        }
        var message = JsonSerializer.Deserialize<T>(messageAsString.Payload) ?? throw new Exception("Could not deserialize message");
        return new RabbitMQMessage<T>(messageAsString.DeliveryTag, message);
    }

    public void DeleteMessage(IConnection connection, string queueName, string deliveryTag)
    {
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: queueName,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        channel.BasicAck(deliveryTag: ulong.Parse(deliveryTag), multiple: false);
    }
}
