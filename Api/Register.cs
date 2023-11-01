using BlobStorageSdk;
using BlobStorageSdk.Models;
using Services;

namespace Api;

public static class Register
{
    public static IServiceCollection RegisterServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddEndpointsApiExplorer();

        services.RegisterBlobStorageSdk(configuration.GetSection(nameof(BlobStorageSdkConfiguration)));
        services.AddSingleton<IFileStorageService, FileStorageService>();

        return services;
    }
}