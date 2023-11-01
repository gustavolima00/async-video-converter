﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Postgres;

namespace Repositories;

public static class Register
{
    private static PostgresConfiguration BuildPostgresConfiguration(IConfigurationSection configuration)
    {
        var config = new PostgresConfiguration();
        config.ConnectionString = configuration.GetSection(nameof(config.ConnectionString)).Value ?? throw new Exception("ConnectionString is required");
        return config;
    }

    public static IServiceCollection RegisterRepositories(
        this IServiceCollection services,
        IConfigurationSection configuration)
    {
        services.AddSingleton(BuildPostgresConfiguration(configuration.GetSection(nameof(PostgresConfiguration))));
        return services;
    }
}