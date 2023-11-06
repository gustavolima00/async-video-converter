using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Repositories.Postgres;

namespace Repositories;

public interface IRawVideosRepository
{
    Task<RawVideo?> TryGetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<RawVideo?> TryGetByPathAsync(string path, CancellationToken cancellationToken = default);
    Task<RawVideo> CreateOrReplaceAsync(RawVideo rawFile, CancellationToken cancellationToken = default);
    Task UpdateConversionStatusAsync(int id, ConversionStatus conversionStatus, CancellationToken cancellationToken = default);
    Task UpdateMetadataAsync(int id, MediaMetadata metadata, CancellationToken cancellationToken = default);
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
        return await _context.RawVideos.FirstOrDefaultAsync(rf => rf.Path == path, cancellationToken);
    }

    public async Task<RawVideo> CreateOrReplaceAsync(RawVideo newFile, CancellationToken cancellationToken = default)
    {
        using var transaction = _context.SupportTransaction
            ? await _context.Database.BeginTransactionAsync(cancellationToken)
            : null;

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

            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            return newFile;
        }
        catch
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            throw;
        }
    }


    public async Task UpdateConversionStatusAsync(int id, ConversionStatus conversionStatus, CancellationToken cancellationToken = default)
    {
        var file = await _context.RawVideos.FindAsync(new object[] { id }, cancellationToken) ?? throw new InvalidOperationException($"Raw file with id {id} not found");
        file.ConversionStatus = conversionStatus;
        _context.RawVideos.Update(file);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateMetadataAsync(int id, MediaMetadata metadata, CancellationToken cancellationToken = default)
    {
        var file = await _context.RawVideos.FindAsync(new object[] { id }, cancellationToken) ?? throw new InvalidOperationException($"Raw file with id {id} not found");
        file.Metadata = metadata;
        _context.RawVideos.Update(file);
        await _context.SaveChangesAsync(cancellationToken);

    }
}