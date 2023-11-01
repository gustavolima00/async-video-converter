using Npgsql;
using Repositories.Postgres;

interface IDatabaseService
{
    Task<DateTime> GetServerTimeAsync();
}

class DatabaseService : IDatabaseService
{
    private readonly DatabaseConnection _databaseConnection;

    public DatabaseService(DatabaseConnection databaseConnection)
    {
        _databaseConnection = databaseConnection;
    }

    public async Task<DateTime> GetServerTimeAsync()
    {
        await using var connection = _databaseConnection.GetConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT NOW()", connection);
        var result = await command.ExecuteScalarAsync() ?? throw new Exception("Failed to get server time");
        return (DateTime)result;
    }
}