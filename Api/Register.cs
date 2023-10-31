namespace Api;

public static class Register
{
    public static IServiceCollection RegisterServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddEndpointsApiExplorer();
        return services;
    }
}