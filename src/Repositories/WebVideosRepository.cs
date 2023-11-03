using System.Text.Json;
using Npgsql;
using NpgsqlTypes;
using Repositories.Models;
using Repositories.Postgres;

namespace Repositories;

public interface IWebVideosRepository
{
    Task<WebVideo> CreateOrReplaceAsync(WebVideo webVideo, CancellationToken cancellationToken = default);
    Task UpdateMetadataAsync(int id, MediaMetadata metadata, CancellationToken cancellationToken = default);
}

class WebVideosRepository : IWebVideosRepository
{
    private readonly IDatabaseConnection _databaseConnection;


    public WebVideosRepository(IDatabaseConnection databaseConnection)
    {
        _databaseConnection = databaseConnection;
    }


    public async Task<WebVideo?> TryGetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = _databaseConnection.GetConnection();
        await connection.OpenAsync(cancellationToken);
        var fields = WebVideo.FieldsNames().Concat(WebVideoSubtitle.FieldsNames());
        string allFields = string.Join(", ", fields);
        await using var command = new NpgsqlCommand($"SELECT {allFields} FROM web_videos from web_videos left join web_video_subtitles on web_video_subtitles.web_video_id = web_videos.id WHERE id = @id ", connection);
        command.Parameters.AddWithValue("id", id);
        var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await WebVideo.BuildFromReader(reader, cancellationToken);
    }

    public async Task<WebVideo> CreateOrReplaceAsync(WebVideo webVideo, CancellationToken cancellationToken = default)
    {
        await using var connection = _databaseConnection.GetConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await using var deleteCommand = new NpgsqlCommand("DELETE FROM web_videos WHERE link = @link AND raw_file_id = @raw_file_id", connection);
            deleteCommand.Parameters.AddWithValue("link", webVideo.Link);
            deleteCommand.Parameters.AddWithValue("raw_file_id", webVideo.RawFileId);
            await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

            await using var insertCommand = new NpgsqlCommand("INSERT INTO web_videos (name, link, raw_file_id) VALUES (@name, @link, @raw_file_id) RETURNING id", connection);
            insertCommand.Parameters.AddWithValue("name", webVideo.Name);
            insertCommand.Parameters.AddWithValue("link", webVideo.Link);
            insertCommand.Parameters.AddWithValue("raw_file_id", webVideo.RawFileId);
            await using var reader = await insertCommand.ExecuteReaderAsync(cancellationToken);
            await reader.ReadAsync(cancellationToken);
            var id = reader.GetInt32(0);
            webVideo.Id = id;
            await reader.CloseAsync();

            await transaction.CommitAsync(cancellationToken);
            return webVideo;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateMetadataAsync(int id, MediaMetadata metadata, CancellationToken cancellationToken = default)
    {
        await using var connection = _databaseConnection.GetConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand("UPDATE web_videos SET metadata = @metadata WHERE id = @id", connection);
        command.AddParameter("id", id);
        command.AddParameter("metadata", JsonSerializer.Serialize(metadata), NpgsqlDbType.Jsonb);
        await command.ExecuteNonQueryAsync(cancellationToken);

    }
}