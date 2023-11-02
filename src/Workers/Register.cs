using Microsoft.Extensions.DependencyInjection;

namespace Workers;

public static class Register
{
    public static void AddWorkers(this IServiceCollection services)
    {
        services.AddHostedService<FillFileMetadataWorker>();
        services.AddHostedService<ConvertFileWorker>();
    }
}