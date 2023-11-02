using Microsoft.Extensions.DependencyInjection;

namespace Services;

public static class Register
{
    public static IServiceCollection RegisterServicesProject(
        this IServiceCollection services)
    {
        services.AddSingleton<IRawFilesService, RawFilesService>();
        services.AddSingleton<IVideoManagerService, VideoManagerService>();
        services.AddSingleton<IQueueService, QueueService>();
        return services;
    }
}