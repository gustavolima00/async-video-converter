using Services;
using Repositories;
using Clients;
using Microsoft.AspNetCore.Http.Features;

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
        services.RegisterClients(configuration.GetProjectConfigurationSection(nameof(Clients)));
        services.RegisterRepositoriesProject(configuration.GetProjectConfigurationSection(nameof(Repositories)));
        services.RegisterServicesProject();

        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.Configure<FormOptions>(options =>
        {
            options.ValueLengthLimit = int.MaxValue;
            options.MultipartBodyLengthLimit = int.MaxValue;
        });

        return services;
    }
}