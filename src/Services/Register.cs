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
        services.AddSingleton<IRawFilesService, RawFilesService>();
        services.AddSingleton<IVideoManagerService, VideoManagerService>();
        services.AddSingleton<IQueueService, QueueService>();
        return services;
    }
}