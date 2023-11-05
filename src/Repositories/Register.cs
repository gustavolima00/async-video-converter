using Microsoft.Extensions.DependencyInjection;
using Repositories.Postgres;

namespace Repositories;

public static class Register
{
    public static IServiceCollection RegisterRepositoriesProject(
         this IServiceCollection services,
         PostgresConfiguration postgresConfiguration)
    {
        services.AddSingleton(postgresConfiguration);
        services.AddSingleton<DatabaseContext>();
        services.AddSingleton<IWebhookRepository, WebhookRepository>();
        services.AddSingleton<IRawFilesRepository, RawFilesRepository>();
        services.AddSingleton<IWebVideosRepository, WebVideosRepository>();
        return services;
    }
}
