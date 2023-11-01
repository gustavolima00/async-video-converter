using Npgsql;
using Repositories.Models;
using Repositories.Postgres;

namespace Repositories;

public interface IRawFilesRepository
{
    Task<IEnumerable<RawFile>> GetAll();
    Task<RawFile?> TryGetById(int id);
    Task Create(RawFile rawFile);
}

class RawFilesRepository : IRawFilesRepository
{
    private readonly IDatabaseConnection _databaseConnection;
    public RawFilesRepository(IDatabaseConnection databaseConnection)
    {
        _databaseConnection = databaseConnection;
    }


    public async Task<IEnumerable<RawFile>> GetAll()
    {
        await using var connection = _databaseConnection.GetConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT id, name, url FROM raw_files", connection);
        var reader = await command.ExecuteReaderAsync();

        var rawFiles = new List<RawFile>();

        while (await reader.ReadAsync())
        {
            rawFiles.Add(new RawFile
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Url = reader.GetString(2),
            });
        }
        return rawFiles;
    }

    public async Task<RawFile?> TryGetById(int id)
    {
        await using var connection = _databaseConnection.GetConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT id, name, url FROM raw_files WHERE id = @id", connection);
        command.Parameters.AddWithValue("id", id);
        var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new RawFile
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Url = reader.GetString(2),
        };
    }

    public async Task Create(RawFile rawFile)
    {
        await using var connection = _databaseConnection.GetConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("INSERT INTO raw_files (name, url) VALUES (@name, @url)", connection);
        command.Parameters.AddWithValue("name", rawFile.Name);
        command.Parameters.AddWithValue("url", rawFile.Url);
        await command.ExecuteNonQueryAsync();
    }
}