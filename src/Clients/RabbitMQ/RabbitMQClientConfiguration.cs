namespace Clients.RabbitMQ;
public class RabbitMQClientConfiguration
{

    public string Hostname { get; set; } = "localhost";
    public int ConnectionRetry { get; set; } = 5;
    public int ConnectionRetryDelay { get; set; } = 5000;
}
