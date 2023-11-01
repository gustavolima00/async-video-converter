using BlobStorageSdk;
using Services;
using Repositories;

namespace Api;

public static class Register
{
    public static IConfigurationSection GetProjectConfigurationSection(this IConfiguration configuration, string project)
    {
        return configuration.GetSection(project + "Configuration");
    }

    public static IServiceCollection RegisterServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Registering other projects
        services.RegisterBlobStorageSdkProject(configuration.GetProjectConfigurationSection(nameof(BlobStorageSdk)));
        services.RegisterRepositoriesProject(configuration.GetProjectConfigurationSection(nameof(Repositories)));
        services.RegisterServicesProject();

        services.AddControllers();
        services.AddEndpointsApiExplorer();

        return services;
    }
}