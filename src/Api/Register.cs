using Services;
using Repositories;
using Clients;
using Microsoft.AspNetCore.Http.Features;
using Workers;
using Repositories.Postgres;
using Services.Configuration;

namespace Api;

public static class Register
{
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
        services.RegisterRepositoriesProject(GetConfiguration<PostgresConfiguration>(configuration));
        services.RegisterServicesProject(GetConfiguration<QueuesConfiguration>(configuration));

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