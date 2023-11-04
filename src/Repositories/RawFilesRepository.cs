using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Repositories.Postgres;

namespace Repositories;

public interface IRawFilesRepository
{
    Task<RawFile?> TryGetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<RawFile?> TryGetByPathAsync(string path, CancellationToken cancellationToken = default);
    Task<RawFile> CreateOrReplaceByPathAsync(string name, string path, CancellationToken cancellationToken = default);
    Task UpdateConversionStatusAsync(int id, ConversionStatus conversionStatus, CancellationToken cancellationToken = default);
    Task UpdateMetadataAsync(int id, MediaMetadata metadata, CancellationToken cancellationToken = default);
}

public class RawFilesRepository : IRawFilesRepository
{
    private readonly DatabaseContext _context;


    public RawFilesRepository(DatabaseContext context)
    {
        _context = context;
    }


    public async Task<RawFile?> TryGetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.RawFiles.FindAsync(new object?[] { id }, cancellationToken: cancellationToken);
    }

    public async Task<RawFile?> TryGetByPathAsync(string path, CancellationToken cancellationToken = default)
    {
        return await _context.RawFiles.FirstOrDefaultAsync(rf => rf.Path == path, cancellationToken);
    }

    public async Task<RawFile> CreateOrReplaceByPathAsync(string name, string path, CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var existingFile = await _context.RawFiles
                .SingleOrDefaultAsync(rf => rf.Path == path, cancellationToken);

            if (existingFile is not null)
            {
                _context.RawFiles.Remove(existingFile);
            }

            var newFile = new RawFile
            {
                Name = name,
                Path = path,
                ConversionStatus = ConversionStatus.NotConverted,
                Metadata = null
            };

            await _context.RawFiles.AddAsync(newFile, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return newFile;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }


    public async Task UpdateConversionStatusAsync(int id, ConversionStatus conversionStatus, CancellationToken cancellationToken = default)
    {
        var file = await _context.RawFiles.FindAsync(new object[] { id }, cancellationToken);

        if (file != null)
        {
            file.ConversionStatus = conversionStatus;
            _context.RawFiles.Update(file);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateMetadataAsync(int id, MediaMetadata metadata, CancellationToken cancellationToken = default)
    {
        var file = await _context.RawFiles.FindAsync(new object[] { id }, cancellationToken);

        if (file != null)
        {
            file.Metadata = metadata;
            _context.RawFiles.Update(file);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
