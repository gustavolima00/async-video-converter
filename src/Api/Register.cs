using Services;
using Repositories;
using Clients;
using Microsoft.AspNetCore.Http.Features;
using Workers;
using Repositories.Postgres;
using Services.Configuration;

namespace Api;

public static class Register
{
    private static TType GetConfiguration<TType>(
     this IConfiguration configuration) where TType : class, new()
    {
        var typeConfig = new TType();
        configuration.Bind(typeConfig.GetType().Name, typeConfig);
        return typeConfig;
    }


    private static ClientsConfiguration BuildClientsConfiguration(IConfiguration configuration)
    {
        var clientsConfiguration = GetConfiguration<ClientsConfiguration>(configuration);
        var blobStorageUrl = Environment.GetEnvironmentVariable("BLOB_STORAGE_URL");
        if (!string.IsNullOrEmpty(blobStorageUrl))
        {
            clientsConfiguration.BlobStorageClientConfiguration.BlobStorageUrl = blobStorageUrl;
        }

        var rabbitMQHostname = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME");
        if (!string.IsNullOrEmpty(rabbitMQHostname))
        {
            clientsConfiguration.RabbitMQClientConfiguration.Hostname = rabbitMQHostname;
        }

        var directoryWithFFmpegAndFFprobe = Environment.GetEnvironmentVariable("DIRECTORY_WITH_FFMPEG_AND_FFPROBE");
        if (!string.IsNullOrEmpty(directoryWithFFmpegAndFFprobe))
        {
            clientsConfiguration.FFmpegClientConfiguration.DirectoryWithFFmpegAndFFprobe = directoryWithFFmpegAndFFprobe;
        }

        Console.WriteLine($"BlobStorageUrl: {clientsConfiguration.BlobStorageClientConfiguration.BlobStorageUrl}");
        Console.WriteLine($"RabbitMQHostname: {clientsConfiguration.RabbitMQClientConfiguration.Hostname}");
        Console.WriteLine($"DirectoryWithFFmpegAndFFprobe: {clientsConfiguration.FFmpegClientConfiguration.DirectoryWithFFmpegAndFFprobe}");
        return clientsConfiguration;
    }

    private static PostgresConfiguration BuildPostgresConfiguration(IConfiguration configuration)
    {
        var postgresConfiguration = GetConfiguration<PostgresConfiguration>(configuration);
        var postgresUrl = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
        if (!string.IsNullOrEmpty(postgresUrl))
        {
            postgresConfiguration.ConnectionString = postgresUrl;
        }
        return postgresConfiguration;
    }

    public static IServiceCollection RegisterServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.RegisterClients(BuildClientsConfiguration(configuration));
        services.RegisterRepositoriesProject(BuildPostgresConfiguration(configuration));
        services.RegisterServicesProject(GetConfiguration<QueuesConfiguration>(configuration));

        services.AddWorkers();

        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.Configure<FormOptions>(options =>
        {
            options.ValueLengthLimit = int.MaxValue;
            options.MultipartBodyLengthLimit = int.MaxValue;
        });

        return services;
    }
}