using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Repositories.Postgres;

namespace Repositories;

public interface IConvertedVideoTracksRepository
{
    Task<ConvertedVideoTrack?> TryGetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ConvertedVideoTrack> CreateOrReplaceAsync(ConvertedVideoTrack convertedVideoTrack, CancellationToken cancellationToken = default);
}


public class ConvertedVideoTracksRepository : IConvertedVideoTracksRepository
{
    private readonly DatabaseContext _context;

    public ConvertedVideoTracksRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<ConvertedVideoTrack?> TryGetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.ConvertedVideoTracks.FirstOrDefaultAsync(rf => rf.Id == id, cancellationToken);
    }

    public async Task<ConvertedVideoTrack> CreateOrReplaceAsync(ConvertedVideoTrack convertedVideoTrack, CancellationToken cancellationToken = default)
    {
        using var transaction = _context.TryBeginTransaction();

        try
        {
            var existingFile = await _context.ConvertedVideoTracks.SingleOrDefaultAsync(rf => rf.Id == convertedVideoTrack.Id, cancellationToken);

            if (existingFile is not null)
            {
                _context.ConvertedVideoTracks.Remove(existingFile);
            }

            await _context.ConvertedVideoTracks.AddAsync(convertedVideoTrack, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.TryCommitAsync(cancellationToken);

            return convertedVideoTrack;
        }
        catch
        {
            await transaction.TryRollbackAsync(cancellationToken);
            throw;
        }
    }
}