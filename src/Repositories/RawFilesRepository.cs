using System.Text.Json;
using Npgsql;
using NpgsqlTypes;
using Repositories.Models;
using Repositories.Postgres;
using Xabe.FFmpeg;

namespace Repositories;

public interface IRawFilesRepository
{
    Task<RawFile?> TryGetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<RawFile?> TryGetByPathAsync(string path, CancellationToken cancellationToken = default);
    Task<RawFile> CreateOrReplaceByPathAsync(string name, string path, CancellationToken cancellationToken = default);
    Task UpdateConversionStatusAsync(int id, ConversionStatus conversionStatus, CancellationToken cancellationToken = default);
    Task UpdateMetadataAsync(int id, MediaMetadata metadata, CancellationToken cancellationToken = default);
}

class RawFilesRepository : IRawFilesRepository
{
    private readonly IDatabaseConnection _databaseConnection;


    public RawFilesRepository(IDatabaseConnection databaseConnection)
    {
        _databaseConnection = databaseConnection;
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

        return RawFile.BuildFromReader(reader);
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

        return RawFile.BuildFromReader(reader);
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
            var rawFile = new RawFile { Name = name, Path = path };
            await using var insertCommand = new NpgsqlCommand("INSERT INTO raw_files (name, path, conversion_status) VALUES (@name, @path, @conversion_status) RETURNING id", connection);
            insertCommand.Parameters.AddWithValue("name", rawFile.Name);
            insertCommand.Parameters.AddWithValue("path", rawFile.Path);
            insertCommand.Parameters.AddWithValue("conversion_status", rawFile.ConversionStatus.ToString());
            await using var reader = await insertCommand.ExecuteReaderAsync(cancellationToken);
            await reader.ReadAsync(cancellationToken);
            var id = reader.GetInt32(0);
            rawFile.Id = id;
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

    public async Task UpdateConversionStatusAsync(int id, ConversionStatus conversionStatus, CancellationToken cancellationToken = default)
    {
        await using var connection = _databaseConnection.GetConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand("UPDATE raw_files SET conversion_status = @conversion_status WHERE id = @id", connection);
        command.AddParameter("id", id);
        command.AddParameter("conversion_status", conversionStatus.ToString());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateMetadataAsync(int id, MediaMetadata metadata, CancellationToken cancellationToken = default)
    {
        await using var connection = _databaseConnection.GetConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand("UPDATE raw_files SET metadata = @metadata WHERE id = @id", connection);
        command.AddParameter("id", id);
        command.AddParameter("metadata", JsonSerializer.Serialize(metadata), NpgsqlDbType.Jsonb);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}