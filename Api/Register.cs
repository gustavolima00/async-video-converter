using BlobStorageSdk;
using Services;
using Repositories;

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

        services.RegisterBlobStorageSdk(configuration.GetSection(nameof(BlobStorageSdk) + "Configuration"));
        services.RegisterRepositories(configuration.GetSection(nameof(Repositories) + "Configuration"));
        services.AddSingleton<IFileStorageService, FileStorageService>();

        return services;
    }
}