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
        services.AddDbContext<DatabaseContext>();
        services.AddScoped<IWebhookRepository, WebhookRepository>();
        services.AddScoped<IRawVideosRepository, RawVideosRepository>();
        services.AddScoped<IConvertedVideosRepository, ConvertedVideosRepository>();
        services.AddScoped<IConvertedVideoTracksRepository, ConvertedVideoTracksRepository>();
        return services;
    }
}
