using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Clients.RabbitMQ;

public interface IRabbitMQClient
{
    void SendMessageAsString(string queueName, string message);
    void SendMessage<T>(string queueName, T message);
    string? ReadMessageAsString(string queueName);
    T? ReadMessage<T>(string queueName);
}

public class RabbitMQClient : IRabbitMQClient
{
    private readonly RabbitMQClientConfiguration _configuration;
    public RabbitMQClient(RabbitMQClientConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void SendMessageAsString(string queueName, string message)
    {
        var factory = new ConnectionFactory() { HostName = _configuration.Hostname };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: queueName,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "",
                             routingKey: queueName,
                             basicProperties: null,
                             body: body);
    }

    public void SendMessage<T>(string queueName, T message)
    {
        var messageAsString = JsonSerializer.Serialize(message);
        SendMessageAsString(queueName, messageAsString);

    }

    public string? ReadMessageAsString(string queueName)
    {
        var factory = new ConnectionFactory() { HostName = _configuration.Hostname };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: queueName,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var data = channel.BasicGet(queueName, true);
        if (data == null)
        {
            return null;
        }
        var message = Encoding.UTF8.GetString(data.Body.ToArray());
        return message;
    }

    public T? ReadMessage<T>(string queueName)
    {
        var messageAsString = ReadMessageAsString(queueName);
        if (messageAsString == null)
        {
            return default;
        }
        var message = JsonSerializer.Deserialize<T>(messageAsString);
        return message;
    }
}
