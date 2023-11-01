using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Repositories.Postgres;

namespace Repositories;

class RawFilesContext : BaseDbContext
{

    public RawFilesContext(PostgresConfiguration configuration) : base(configuration)
    {
    }

    public DbSet<RawFile> RawFiles { get; set; }
}

public interface IRawFilesRepository
{
    Task<RawFile?> TryGetRawFile(int id);
    Task<RawFile> AddRawFile(RawFile rawFile);
    Task<RawFile> UpdateRawFile(RawFile rawFile);
    Task DeleteRawFile(int id);
}

class RawFilesRepository : IRawFilesRepository
{
    private readonly RawFilesContext _context;
    public RawFilesRepository(RawFilesContext context)
    {
        _context = context;
    }

    public async Task<RawFile?> TryGetRawFile(int id)
    {
        return await _context.RawFiles.FindAsync(id);
    }

    public async Task<RawFile> AddRawFile(RawFile rawFile)
    {
        await _context.RawFiles.AddAsync(rawFile);
        await _context.SaveChangesAsync();
        return rawFile;
    }

    public async Task<RawFile> UpdateRawFile(RawFile rawFile)
    {
        _context.RawFiles.Update(rawFile);
        await _context.SaveChangesAsync();
        return rawFile;
    }

    public async Task DeleteRawFile(int id)
    {
        var rawFile = await _context.RawFiles.FindAsync(id);
        if (rawFile is not null)
        {
            _context.RawFiles.Remove(rawFile);
            await _context.SaveChangesAsync();
        }
    }
}