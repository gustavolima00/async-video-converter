using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Repositories.Postgres;

namespace Repositories;

public interface IConvertedVideosRepository
{
    Task<IEnumerable<ConvertedVideo>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ConvertedVideo> GetOrCreateByRawVideoIdAsync(int rawVideoId, CancellationToken cancellationToken = default);
    Task<ConvertedVideo?> TryGetByIdAsync(int id, CancellationToken cancellationToken = default);

    // Subtitles
    Task<ConvertedSubtitle?> TryGetSubtitleByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ConvertedSubtitle> CreateOrReplaceConvertedSubtitleAsync(ConvertedSubtitle convertedSubtitle, CancellationToken cancellationToken = default);
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
        return await _context.ConvertedVideos
            .Include(rf => rf.Subtitles)
            .Include(rf => rf.Streams)
            .ToListAsync(cancellationToken);
    }

    private async Task<ConvertedVideo> CreateOrReplaceAsync(ConvertedVideo convertedVideo, CancellationToken cancellationToken = default)
    {
        using var transaction = _context.TryBeginTransaction();

        try
        {
            var existingFile = await _context.ConvertedVideos.SingleOrDefaultAsync(rf => rf.RawVideoId == convertedVideo.RawVideoId, cancellationToken);

            if (existingFile is not null)
            {
                _context.ConvertedVideos.Remove(existingFile);
            }

            await _context.ConvertedVideos.AddAsync(convertedVideo, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.TryCommitAsync(cancellationToken);

            return convertedVideo;
        }
        catch
        {
            await transaction.TryRollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<ConvertedVideo?> TryGetByRawVideoIdAsync(int rawVideoId, CancellationToken cancellationToken = default)
    {
        return await _context.ConvertedVideos.FirstOrDefaultAsync(rf => rf.RawVideoId == rawVideoId, cancellationToken);
    }

    public async Task<ConvertedVideo> GetOrCreateByRawVideoIdAsync(int rawVideoId, CancellationToken cancellationToken = default)
    {
        var convertedVideo = await TryGetByRawVideoIdAsync(rawVideoId, cancellationToken);

        convertedVideo ??= await CreateOrReplaceAsync(new()
        {
            RawVideoId = rawVideoId
        }, cancellationToken);

        return convertedVideo;
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
}
