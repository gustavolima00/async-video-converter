using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Text;

namespace Clients.RabbitMQ;
public class RabbitMQClientConfiguration
{

    public string Hostname { get; set; } = "localhost";

    public static RabbitMQClientConfiguration FromConfiguration(IConfigurationSection configurationSection)
    {
        var configuration = new RabbitMQClientConfiguration();
        configuration.Hostname =
            configurationSection.GetSection(nameof(Hostname)).Value
            ?? configuration.Hostname;

        return configuration;
    }
}
