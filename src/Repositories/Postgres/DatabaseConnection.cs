using Npgsql;

namespace Repositories.Postgres;

public interface IDatabaseConnection
{
    NpgsqlConnection GetConnection();
}

public class DatabaseConnection : IDatabaseConnection
{
    private readonly PostgresConfiguration _configuration;

    public DatabaseConnection(PostgresConfiguration configuration)
    {
        _configuration = configuration;
    }

    public NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_configuration.ConnectionString);
    }
}
