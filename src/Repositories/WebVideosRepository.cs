using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Repositories.Postgres;

namespace Repositories;

public interface IWebVideosRepository
{
    Task<IEnumerable<WebVideo>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WebVideo> CreateOrReplaceAsync(WebVideo webVideo, CancellationToken cancellationToken = default);
    Task UpdateMetadataAsync(int id, MediaMetadata metadata, CancellationToken cancellationToken = default);
    Task<WebVideo?> TryGetByIdAsync(int id, CancellationToken cancellationToken = default);
}

public class WebVideosRepository : IWebVideosRepository
{
    private readonly DatabaseContext _context;


    public WebVideosRepository(DatabaseContext context)
    {
        _context = context;
    }


    public async Task<WebVideo?> TryGetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.WebVideos.FindAsync(new object?[] { id }, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<WebVideo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.WebVideos.ToListAsync(cancellationToken);
    }

    public async Task<WebVideo> CreateOrReplaceAsync(WebVideo webVideo, CancellationToken cancellationToken = default)
    {
        using var transaction = _context.SupportTransaction
            ? await _context.Database.BeginTransactionAsync(cancellationToken)
            : null;

        try
        {
            var existingFile = await _context.WebVideos.SingleOrDefaultAsync(rf => rf.Path == webVideo.Path, cancellationToken);

            if (existingFile is not null)
            {
                _context.WebVideos.Remove(existingFile);
            }

            await _context.WebVideos.AddAsync(webVideo, cancellationToken);
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
        var webVideo = await _context.WebVideos.FindAsync(new object?[] { id }, cancellationToken: cancellationToken) ?? throw new ArgumentException($"No web video with id {id} exists");
        webVideo.Metadata = metadata;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
