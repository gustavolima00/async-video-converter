using Clients.BlobStorage;
using Clients.FFmpeg;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Clients;

public static class Register
{
    public static IServiceCollection RegisterClients(
        this IServiceCollection services,
        IConfigurationSection configuration)
    {
        services.AddHttpClient();
        services.AddSingleton(
            BlobStorageClientConfiguration.Build(
                configuration.GetSection(nameof(BlobStorageClientConfiguration))
            )
        );
        services.AddSingleton<IBlobStorageClient, BlobStorageClient>();

        services.AddSingleton(
            FFmpegClientConfiguration.FromConfiguration(
                configuration.GetSection(nameof(FFmpegClientConfiguration))
            )
        );
        services.AddSingleton<IFFmpegClient, FFmpegClient>();
        return services;
    }
}