using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Repositories.Postgres;

namespace Repositories;

public interface IConvertedVideosRepository
{
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

    private async Task<ConvertedVideo> CreateOrReplaceAsync(ConvertedVideo convertedVideo, CancellationToken cancellationToken = default)
    {
        var existingVideo = await _context.ConvertedVideos
            .AsNoTracking()
            .FirstOrDefaultAsync(cv => cv.RawVideoId == convertedVideo.RawVideoId, cancellationToken);

        if (existingVideo is not null)
        {
            _context.Entry(existingVideo).CurrentValues.SetValues(convertedVideo);
        }
        else
        {
            await _context.ConvertedVideos.AddAsync(convertedVideo, cancellationToken);
        }
        await _context.SaveChangesAsync(cancellationToken);
        return convertedVideo;
    }

    private async Task<ConvertedVideo?> TryGetByRawVideoIdAsync(int rawVideoId, CancellationToken cancellationToken = default)
    {
        return await _context.ConvertedVideos.FirstOrDefaultAsync(rf => rf.RawVideoId == rawVideoId, cancellationToken);
    }

    public async Task<ConvertedVideo> GetOrCreateByRawVideoIdAsync(int rawVideoId, CancellationToken cancellationToken = default)
    {
        var convertedVideo = await TryGetByRawVideoIdAsync(rawVideoId, cancellationToken);
        if (convertedVideo is not null)
        {
            return convertedVideo;
        }

        return await CreateOrReplaceAsync(new()
        {
            RawVideoId = rawVideoId
        }, cancellationToken);
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
