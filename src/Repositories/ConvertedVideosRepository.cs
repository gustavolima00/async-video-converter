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
    Task<ConvertedVideo?> TryGetByRawVideoIdAsync(int rawVideoId, CancellationToken cancellationToken = default);

    // Subtitles
    Task<ConvertedSubtitle?> TryGetSubtitleByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ConvertedSubtitle> CreateOrReplaceConvertedSubtitleAsync(ConvertedSubtitle convertedSubtitle, CancellationToken cancellationToken = default);
    Task UpdateSubtitleMetadataAsync(int id, MediaMetadata metadata, CancellationToken cancellationToken = default);
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
        return await _context.ConvertedVideos.FirstOrDefaultAsync(rf => rf.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ConvertedVideo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ConvertedVideos.Include(rf => rf.Subtitles).ToListAsync(cancellationToken);
    }

    public async Task<ConvertedVideo> CreateOrReplaceAsync(ConvertedVideo webVideo, CancellationToken cancellationToken = default)
    {
        using var transaction = _context.TryBeginTransaction();

        try
        {
            var existingFile = await _context.ConvertedVideos.SingleOrDefaultAsync(rf => rf.Path == webVideo.Path, cancellationToken);

            if (existingFile is not null)
            {
                _context.ConvertedVideos.Remove(existingFile);
            }

            await _context.ConvertedVideos.AddAsync(webVideo, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.TryCommitAsync(cancellationToken);

            return webVideo;
        }
        catch
        {
            await transaction.TryRollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateMetadataAsync(int id, MediaMetadata metadata, CancellationToken cancellationToken = default)
    {
        var webVideo = await _context.ConvertedVideos.FindAsync(new object?[] { id }, cancellationToken: cancellationToken) ?? throw new ArgumentException($"No web video with id {id} exists");
        webVideo.Metadata = metadata;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ConvertedVideo?> TryGetByRawVideoIdAsync(int rawVideoId, CancellationToken cancellationToken = default)
    {
        return await _context.ConvertedVideos.FirstOrDefaultAsync(rf => rf.RawVideoId == rawVideoId, cancellationToken);
    }

    // Subtitles
    public async Task<ConvertedSubtitle?> TryGetSubtitleByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.ConvertedSubtitles.FirstOrDefaultAsync(rf => rf.Id == id, cancellationToken);
    }

    public async Task<ConvertedSubtitle> CreateOrReplaceConvertedSubtitleAsync(ConvertedSubtitle convertedSubtitle, CancellationToken cancellationToken = default)
    {
        using var transaction = _context.TryBeginTransaction();

        try
        {
            var existingFile = await _context.ConvertedSubtitles.SingleOrDefaultAsync(
                    rf => rf.Path == convertedSubtitle.Path
                    && convertedSubtitle.ConvertedVideoId == rf.ConvertedVideoId,
                    cancellationToken);

            if (existingFile is not null)
            {
                _context.ConvertedSubtitles.Remove(existingFile);
            }

            await _context.ConvertedSubtitles.AddAsync(convertedSubtitle, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.TryCommitAsync(cancellationToken);

            return convertedSubtitle;
        }
        catch
        {
            await transaction.TryRollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateSubtitleMetadataAsync(int id, MediaMetadata metadata, CancellationToken cancellationToken = default)
    {
        var subtitle = await _context.ConvertedSubtitles.FindAsync(new object?[] { id }, cancellationToken: cancellationToken) ?? throw new ArgumentException($"No subtitle with id {id} exists");
        subtitle.Metadata = metadata;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
