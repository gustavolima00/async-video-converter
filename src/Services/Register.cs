using Microsoft.Extensions.DependencyInjection;
using Services.Configuration;

namespace Services;

public static class Register
{
    public static IServiceCollection RegisterServicesProject(
        this IServiceCollection services,
        QueuesConfiguration queuesConfiguration)
    {
        services.AddSingleton(queuesConfiguration);
        services.AddSingleton<IRawVideoService, RawVideosService>();
        services.AddSingleton<IMediaService, MediaService>();
        services.AddSingleton<IWebVideoService, WebVideoService>();
        services.AddSingleton<IQueueService, QueueService>();
        services.AddSingleton<IWebhookService, WebhookService>();
        return services;
    }
}
