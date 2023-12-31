using Microsoft.Extensions.DependencyInjection;

namespace Workers;

public static class Register
{
    public static void AddWorkers(this IServiceCollection services)
    {
        services.AddHostedService<ExtractSubtitlesWorker>();
        services.AddHostedService<ExtractVideoTracksWorker>();
        services.AddHostedService<WebhookWorker>();
    }
}
