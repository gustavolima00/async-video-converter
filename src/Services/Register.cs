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
        services.AddScoped<IRawVideoService, RawVideosService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IVideoConversionService, VideoConversionService>();
        services.AddSingleton<IQueueService, QueueService>();
        services.AddScoped<IWebhookService, WebhookService>();
        return services;
    }
}
