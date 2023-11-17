using Microsoft.Extensions.DependencyInjection;

namespace Workers;

public static class Register
{
    public static void AddWorkers(this IServiceCollection services)
    {
        services.AddHostedService<ConvertFileWorker>();
        services.AddHostedService<ExtractSubtitlesWorker>();
        services.AddHostedService<WebhookWorker>();
    }
}
