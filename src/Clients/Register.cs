using Clients.BlobStorage;
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
        var blobStorageClientConfiguration = configuration.GetSection(nameof(BlobStorageClientConfiguration));
        services.AddSingleton(
            BlobStorageClientConfiguration.Build(blobStorageClientConfiguration)
        );
        services.AddSingleton<IBlobStorageClient, BlobStorageClient>();
        return services;
    }
}