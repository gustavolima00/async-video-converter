using Services;
using Repositories;
using Clients;
using Microsoft.AspNetCore.Http.Features;
using Workers;

namespace Api;

public static class Register
{
    public static IConfigurationSection GetProjectConfigurationSection(this IConfiguration configuration, string project)
    {
        return configuration.GetSection(project + "Configuration");
    }

    private static TType GetConfiguration<TType>(
    this IConfiguration configuration) where TType : class, new()
    {
        var typeConfig = new TType();
        configuration.Bind(typeConfig.GetType().Name, typeConfig);
        return typeConfig;
    }

    public static IServiceCollection RegisterServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.RegisterClients(GetConfiguration<ClientsConfiguration>(configuration));
        services.RegisterRepositoriesProject(configuration.GetProjectConfigurationSection(nameof(Repositories)));
        services.RegisterServicesProject();

        services.AddWorkers();

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