using Clients.BlobStorage;
using Clients.FFmpeg;
using Clients.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;


namespace Clients;

public static class Register
{
    public static IServiceCollection RegisterClients(
        this IServiceCollection services,
        ClientsConfiguration configuration)
    {
        services.AddHttpClient();
        services.AddSingleton(configuration.BlobStorageClientConfiguration);
        services.AddSingleton<IBlobStorageClient, BlobStorageClient>();

        services.AddSingleton(configuration.FFmpegClientConfiguration);
        services.AddSingleton<IFFmpegClient, FFmpegClient>();

        services.AddSingleton(configuration.RabbitMQClientConfiguration);
        services.AddSingleton<IRabbitMQClient, RabbitMQClient>();
        return services;
    }
}