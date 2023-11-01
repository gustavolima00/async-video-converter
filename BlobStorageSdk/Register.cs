using BlobStorageSdk.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlobStorageSdk;

public static class Register
{
    private static BlobStorageSdkConfiguration BuildConfigurationInstance(IConfigurationSection configuration)
    {
        var config = new BlobStorageSdkConfiguration();
        config.BlobStorageUrl = configuration.GetSection(nameof(config.BlobStorageUrl)).Value ?? "";
        if (string.IsNullOrEmpty(config.BlobStorageUrl))
            throw new Exception("BlobStorageUrl is required");
        return config;
    }

    public static IServiceCollection RegisterBlobStorageSdkProject(
        this IServiceCollection services,
        IConfigurationSection configuration)
    {
        services.AddHttpClient();
        services.AddSingleton(BuildConfigurationInstance(configuration));
        services.AddSingleton<IBlobStorageApi, BlobStorageApi>();
        return services;
    }
}