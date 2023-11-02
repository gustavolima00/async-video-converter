using System.Text.Json;
using Npgsql;
using NpgsqlTypes;
using Repositories.Models;
using Repositories.Postgres;

namespace Repositories;

public interface IRawFilesRepository
{
    Task<RawFile?> TryGetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<RawFile?> TryGetByPathAsync(string path, CancellationToken cancellationToken = default);
    Task<RawFile> CreateOrReplaceByPathAsync(string name, string path, CancellationToken cancellationToken = default);
    Task UpdateConvertedPathAsync(int id, string convertedPath, CancellationToken cancellationToken = default);
    Task UpdateMetadataAsync(int id, Metadata metadata, CancellationToken cancellationToken = default);
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
            ConvertedPath = reader.IsDBNull(3) ? null : reader.GetString(3),
            Metadata = reader.IsDBNull(4) ? null : JsonSerializer.Deserialize<Metadata>(reader.GetString(4))
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

        await using var command = new NpgsqlCommand("SELECT * FROM raw_files WHERE path = @path", connection);
        command.Parameters.AddWithValue("path", path);
        var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadRawFile(reader);
    }

    public async Task<RawFile> CreateOrReplaceByPathAsync(string name, string path, CancellationToken cancellationToken = default)
    {
        await using var connection = _databaseConnection.GetConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await using var deleteCommand = new NpgsqlCommand("DELETE FROM raw_files WHERE path = @path", connection);
            deleteCommand.Parameters.AddWithValue("path", path);
            await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

            await using var insertCommand = new NpgsqlCommand("INSERT INTO raw_files (name, path) VALUES (@name, @path) RETURNING *", connection);
            insertCommand.Parameters.AddWithValue("name", name);
            insertCommand.Parameters.AddWithValue("path", path);
            await using var reader = await insertCommand.ExecuteReaderAsync(cancellationToken);

            var rawFile = ReadRawFile(reader);
            await reader.CloseAsync();

            await transaction.CommitAsync(cancellationToken);
            return rawFile;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }


    public async Task UpdateConvertedPathAsync(int id, string convertedPath, CancellationToken cancellationToken = default)
    {
        await using var connection = _databaseConnection.GetConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand("UPDATE raw_files SET converted_path = @converted_path WHERE id = @id", connection);
        command.AddParameter("id", id);
        command.AddParameter("converted_path", convertedPath);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateMetadataAsync(int id, Metadata metadata, CancellationToken cancellationToken = default)
    {
        await using var connection = _databaseConnection.GetConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand("UPDATE raw_files SET metadata = @metadata WHERE id = @id", connection);
        command.AddParameter("id", id);
        command.AddParameter("metadata", JsonSerializer.Serialize(metadata), NpgsqlDbType.Jsonb);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}