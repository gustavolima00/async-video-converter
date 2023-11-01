using Npgsql;
namespace Repositories.Postgres;

interface IDatabaseConnection
{
    NpgsqlConnection GetConnection();
}

class DatabaseConnection : IDatabaseConnection
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