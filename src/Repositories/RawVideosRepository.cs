using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Repositories.Postgres;

namespace Repositories;

public interface IRawVideosRepository
{
    // Videos
    Task<RawVideo?> TryGetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<RawVideo?> TryGetByPathAsync(string path, CancellationToken cancellationToken = default);
    Task<RawVideo> CreateOrReplaceAsync(RawVideo rawFile, CancellationToken cancellationToken = default);
    Task UpdateAsync(RawVideo rawVideo, CancellationToken cancellationToken = default);

    // Subtitles
    Task<RawSubtitle> CreateOrReplaceRawSubtitleAsync(RawSubtitle rawSubtitle, CancellationToken cancellationToken = default);
    Task<RawSubtitle?> TryGetSubtitleByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<RawSubtitle?> TryGetSubtitleByPathAsync(string path, CancellationToken cancellationToken = default);
}

public class RawVideosRepository : IRawVideosRepository
{
    private readonly DatabaseContext _context;


    public RawVideosRepository(DatabaseContext context)
    {
        _context = context;
    }


    public async Task<RawVideo?> TryGetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.RawVideos.FindAsync(new object?[] { id }, cancellationToken: cancellationToken);
    }

    public async Task<RawVideo?> TryGetByPathAsync(string path, CancellationToken cancellationToken = default)
    {
        return await _context.RawVideos
            .Include(rf => rf.Subtitles)
            .Include(rf => rf.ConvertedVideo)
            .Include(rf => rf.ConvertedVideo.Subtitles)
            .Include(rf => rf.ConvertedVideo.Streams)
            .FirstOrDefaultAsync(rf => rf.Path == path, cancellationToken);
    }

    public async Task<RawVideo> CreateOrReplaceAsync(RawVideo newFile, CancellationToken cancellationToken = default)
    {
        using var transaction = _context.TryBeginTransaction();
        try
        {
            var existingFile = await _context.RawVideos.FirstOrDefaultAsync(
                rf => rf.Path == newFile.Path &&
                rf.UserUuid == newFile.UserUuid,
                cancellationToken);

            if (existingFile is not null)
            {
                _context.RawVideos.Remove(existingFile);
            }

            await _context.RawVideos.AddAsync(newFile, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.TryCommitAsync(cancellationToken);

            return newFile;
        }
        catch
        {
            await transaction.TryRollbackAsync(cancellationToken);
            throw;
        }
    }


    public async Task UpdateAsync(RawVideo rawVideo, CancellationToken cancellationToken = default)
    {
        _context.RawVideos.Update(rawVideo);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<RawSubtitle> CreateOrReplaceRawSubtitleAsync(RawSubtitle rawSubtitle, CancellationToken cancellationToken = default)
    {
        using var transaction = _context.TryBeginTransaction();
        try
        {
            var existingFile = await _context.RawSubtitles.FirstOrDefaultAsync(
                rf => rf.Path == rawSubtitle.Path &&
                rf.RawVideoId == rawSubtitle.RawVideoId,
                cancellationToken);

            if (existingFile is not null)
            {
                _context.RawSubtitles.Remove(existingFile);
            }

            await _context.RawSubtitles.AddAsync(rawSubtitle, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.TryCommitAsync(cancellationToken);

            return rawSubtitle;
        }
        catch
        {

            await transaction.TryRollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<RawSubtitle?> TryGetSubtitleByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.RawSubtitles.FindAsync(new object?[] { id }, cancellationToken: cancellationToken);
    }

    public async Task<RawSubtitle?> TryGetSubtitleByPathAsync(string path, CancellationToken cancellationToken = default)
    {
        return await _context.RawSubtitles.FirstOrDefaultAsync(rf => rf.Path == path, cancellationToken);
    }
}
