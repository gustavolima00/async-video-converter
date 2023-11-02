namespace Clients.RabbitMQ.Models;

public class RabbitMQMessage<T>
{
    public string DeliveryTag { get; set; }
    public T Payload { get; set; }

    public RabbitMQMessage(string deliveryTag, T payload)
    {
        DeliveryTag = deliveryTag;
        Payload = payload;
    }
}