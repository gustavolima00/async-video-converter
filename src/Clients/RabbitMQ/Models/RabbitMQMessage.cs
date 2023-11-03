namespace Clients.RabbitMQ.Models;

public class RabbitMQMessage<T>
{
    public ulong DeliveryTag { get; set; }
    public T Payload { get; set; }

    public RabbitMQMessage(ulong deliveryTag, T payload)
    {
        DeliveryTag = deliveryTag;
        Payload = payload;
    }
}
