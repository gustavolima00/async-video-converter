using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Repositories.Postgres;

namespace Repositories;

public interface IConvertedVideosRepository
{
    Task<IEnumerable<ConvertedVideo>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ConvertedVideo> CreateOrReplaceAsync(ConvertedVideo webVideo, CancellationToken cancellationToken = default);
    Task UpdateMetadataAsync(int id, MediaMetadata metadata, CancellationToken cancellationToken = default);
    Task<ConvertedVideo?> TryGetByIdAsync(int id, CancellationToken cancellationToken = default);
}

public class ConvertedVideosRepository : IConvertedVideosRepository
{
    private readonly DatabaseContext _context;


    public ConvertedVideosRepository(DatabaseContext context)
    {
        _context = context;
    }


    public async Task<ConvertedVideo?> TryGetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.ConvertedVideos.FindAsync(new object?[] { id }, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<ConvertedVideo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ConvertedVideos.ToListAsync(cancellationToken);
    }

    public async Task<ConvertedVideo> CreateOrReplaceAsync(ConvertedVideo webVideo, CancellationToken cancellationToken = default)
    {
        using var transaction = _context.SupportTransaction
            ? await _context.Database.BeginTransactionAsync(cancellationToken)
            : null;

        try
        {
            var existingFile = await _context.ConvertedVideos.SingleOrDefaultAsync(rf => rf.Path == webVideo.Path, cancellationToken);

            if (existingFile is not null)
            {
                _context.ConvertedVideos.Remove(existingFile);
            }

            await _context.ConvertedVideos.AddAsync(webVideo, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            transaction?.Commit();

            return webVideo;
        }
        catch
        {
            transaction?.Rollback();
            throw;
        }
    }

    public async Task UpdateMetadataAsync(int id, MediaMetadata metadata, CancellationToken cancellationToken = default)
    {
        var webVideo = await _context.ConvertedVideos.FindAsync(new object?[] { id }, cancellationToken: cancellationToken) ?? throw new ArgumentException($"No web video with id {id} exists");
        webVideo.Metadata = metadata;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
