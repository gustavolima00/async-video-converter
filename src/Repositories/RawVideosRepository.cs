using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Repositories.Postgres;

namespace Repositories;

public interface IRawVideosRepository
{
    Task<RawVideo?> TryGetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<RawVideo?> TryGetByPathAsync(string path, CancellationToken cancellationToken = default);
    Task<RawVideo> CreateOrReplaceAsync(RawVideo rawFile, CancellationToken cancellationToken = default);
    Task<RawVideo> GetByUuidAsync(Guid uuid, CancellationToken cancellationToken = default);
    Task UpdateAsync(RawVideo rawVideo, CancellationToken cancellationToken = default);
    Task<IEnumerable<RawVideo>> GetByUserUuidAsync(Guid userUuid, CancellationToken cancellationToken = default);
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
        return await _context.RawVideos
                    .Include(rf => rf.ConvertedVideo)
                    .Include(rf => rf.ConvertedVideo.Subtitles)
                    .Include(rf => rf.ConvertedVideo.Streams)
                    .FirstOrDefaultAsync(rf => rf.Id == id, cancellationToken);
    }

    public async Task<RawVideo?> TryGetByPathAsync(string path, CancellationToken cancellationToken = default)
    {
        return await _context.RawVideos
            .Include(rf => rf.ConvertedVideo)
            .Include(rf => rf.ConvertedVideo.Subtitles)
            .Include(rf => rf.ConvertedVideo.Streams)
            .FirstOrDefaultAsync(rf => rf.Path == path, cancellationToken);
    }

    public async Task<RawVideo> GetByUuidAsync(Guid uuid, CancellationToken cancellationToken = default)
    {
        var rawVideo = await _context.RawVideos
            .Include(rf => rf.ConvertedVideo)
            .Include(rf => rf.ConvertedVideo.Subtitles)
            .Include(rf => rf.ConvertedVideo.Streams)
            .FirstOrDefaultAsync(rf => rf.Uuid == uuid, cancellationToken);
        return rawVideo ?? throw new Exception($"Raw video with uuid {uuid} not found");
    }

    public async Task<IEnumerable<RawVideo>> GetByUserUuidAsync(Guid userUuid, CancellationToken cancellationToken = default)
    {
        return await _context.RawVideos
            .Include(rf => rf.ConvertedVideo)
            .Include(rf => rf.ConvertedVideo.Subtitles)
            .Include(rf => rf.ConvertedVideo.Streams)
            .Where(rf => rf.UserUuid == userUuid)
            .ToListAsync(cancellationToken);
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
}
