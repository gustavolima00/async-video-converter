using Npgsql;
using Repositories.Models;
using Repositories.Postgres;

namespace Repositories;

public interface IRawFilesRepository
{
    Task<RawFile?> TryGetById(int id, CancellationToken cancellationToken = default);
    Task<RawFile?> TryGetByPath(string path, CancellationToken cancellationToken = default);
    Task<RawFile> Create(RawFile rawFile, CancellationToken cancellationToken = default);
}

class RawFilesRepository : IRawFilesRepository
{
    private readonly IDatabaseConnection _databaseConnection;
    private readonly string _rawFileFields = "id, name, path";

    public RawFilesRepository(IDatabaseConnection databaseConnection)
    {
        _databaseConnection = databaseConnection;
    }

    private static RawFile ReadRawFile(NpgsqlDataReader reader)
    {
        return new RawFile
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Path = reader.GetString(2),
        };
    }

    public async Task<RawFile?> TryGetById(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = _databaseConnection.GetConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand("SELECT " + _rawFileFields + " FROM raw_files WHERE id = @id", connection);
        command.Parameters.AddWithValue("id", id);
        var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadRawFile(reader);
    }

    public async Task<RawFile?> TryGetByPath(string path, CancellationToken cancellationToken = default)
    {
        await using var connection = _databaseConnection.GetConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand("SELECT " + _rawFileFields + " FROM raw_files WHERE path = @path LIMIT 1", connection);
        command.Parameters.AddWithValue("path", path);
        var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadRawFile(reader);
    }

    public async Task<RawFile> Create(RawFile rawFile, CancellationToken cancellationToken = default)
    {
        await using var connection = _databaseConnection.GetConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand("INSERT INTO raw_files (name, path) VALUES (@name, @path) RETURNING id", connection);
        command.Parameters.AddWithValue("name", rawFile.Name);
        command.Parameters.AddWithValue("path", rawFile.Path);
        var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new Exception("Failed to create raw file");
        }

        rawFile.Id = reader.GetInt32(0);
        return rawFile;
    }
}