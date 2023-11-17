using System.Text;
using System.Text.Json;
using Clients.RabbitMQ.Models;
using RabbitMQ.Client;

namespace Clients.RabbitMQ;

public interface IRabbitMQClient
{
    void SendMessage<T>(string queueName, T message);
    RabbitMQMessage<T>? ReadMessage<T>(string queueName);
    void AckMessage(ulong messageId);
}

public class RabbitMQClient : IRabbitMQClient
{
    private readonly RabbitMQClientConfiguration _configuration;
    private readonly IConnection _connection;
    private readonly IModel _model;
    public RabbitMQClient(RabbitMQClientConfiguration configuration)
    {
        _configuration = configuration;
        _connection = CreateConnection();
        _model = CreateModel();
    }

    public IConnection CreateConnection()
    {
        for (var i = 0; i < _configuration.ConnectionRetry; i++)
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = _configuration.Hostname };
                return factory.CreateConnection();
            }
            catch (Exception)
            {
                Thread.Sleep(_configuration.ConnectionRetryDelay);
            }
        }
        throw new Exception("Could not connect to RabbitMQ");
    }

    public IModel CreateModel()
    {
        return _connection.CreateModel();
    }

    public void SendMessageAsString(string queueName, string message)
    {
        _model.QueueDeclare(queue: queueName,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var body = Encoding.UTF8.GetBytes(message);

        _model.BasicPublish(exchange: "",
                             routingKey: queueName,
                             basicProperties: null,
                             body: body);
    }

    public void SendMessage<T>(string queueName, T message)
    {
        var messageAsString = JsonSerializer.Serialize(message);
        SendMessageAsString(queueName, messageAsString);

    }

    public RabbitMQMessage<string>? ReadMessageAsString(string queueName)
    {
        _model.QueueDeclare(queue: queueName,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var data = _model.BasicGet(queueName, false);
        if (data == null)
        {
            return null;
        }
        var message = Encoding.UTF8.GetString(data.Body.ToArray());
        var deliveryTag = data.DeliveryTag;
        return new RabbitMQMessage<string>(deliveryTag, message);
    }

    public RabbitMQMessage<T>? ReadMessage<T>(string queueName)
    {
        var messageAsString = ReadMessageAsString(queueName);
        if (messageAsString == null)
        {
            return null;
        }
        var message = JsonSerializer.Deserialize<T>(messageAsString.Payload) ?? throw new Exception("Could not deserialize message");
        return new RabbitMQMessage<T>(messageAsString.DeliveryTag, message);
    }

    public void AckMessage(ulong messageId)
    {
        _model.BasicAck(messageId, false);
    }
}
