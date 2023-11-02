using System.Text.Json;
using Npgsql;
using Repositories.Models;
using Repositories.Postgres;

namespace Repositories;

public interface IRawFilesRepository
{
    Task<RawFile?> TryGetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<RawFile?> TryGetByPathAsync(string path, CancellationToken cancellationToken = default);
    Task<RawFile> CreateAsync(RawFile rawFile, CancellationToken cancellationToken = default);
    Task UpdateAsync(RawFile rawFile, CancellationToken cancellationToken = default);
}

class RawFilesRepository : IRawFilesRepository
{
    private readonly IDatabaseConnection _databaseConnection;


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
            Metadata = JsonSerializer.Deserialize<Metadata>(reader.GetString(3))
        };
    }

    public async Task<RawFile?> TryGetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = _databaseConnection.GetConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand("SELECT * FROM raw_files WHERE id = @id", connection);
        command.Parameters.AddWithValue("id", id);
        var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadRawFile(reader);
    }

    public async Task<RawFile?> TryGetByPathAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var connection = _databaseConnection.GetConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand("SELECT * FROM raw_files WHERE path = @path LIMIT 1", connection);
        command.Parameters.AddWithValue("path", path);
        var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadRawFile(reader);
    }

    public async Task<RawFile> CreateAsync(RawFile rawFile, CancellationToken cancellationToken = default)
    {
        await using var connection = _databaseConnection.GetConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand("INSERT INTO raw_files (name, path, metadata) VALUES (@name, @path, @metadata) RETURNING id", connection);
        command.Parameters.AddWithValue("name", rawFile.Name);
        command.Parameters.AddWithValue("path", rawFile.Path);
        command.Parameters.AddWithValue("metadata", JsonSerializer.Serialize(rawFile.Metadata));
        var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new Exception("Failed to create raw file");
        }

        rawFile.Id = reader.GetInt32(0);
        return rawFile;
    }

    public async Task UpdateAsync(RawFile rawFile, CancellationToken cancellationToken = default)
    {
        await using var connection = _databaseConnection.GetConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand("UPDATE raw_files SET name = @name, path = @path, metadata = @metadata WHERE id = @id", connection);
        command.Parameters.AddWithValue("id", rawFile.Id);
        command.Parameters.AddWithValue("name", rawFile.Name);
        command.Parameters.AddWithValue("path", rawFile.Path);
        command.Parameters.AddWithValue("metadata", JsonSerializer.Serialize(rawFile.Metadata));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}