using Xunit;
using System.Threading.Tasks;
using Repositories;
using Repositories.Postgres;
using Repositories.Models;
namespace Tests.Repositories;

public class RawFilesRepositoriesTests
{
    private readonly IRawFilesRepository _repository;
    private readonly DatabaseContext _context;
    public RawFilesRepositoriesTests()
    {
        var postgresConfiguration = new PostgresConfiguration
        {
            UseInMemoryDatabase = true
        };
        _context = new DatabaseContext(postgresConfiguration);
        _repository = new RawFilesRepository(_context);
    }

    [Fact]
    public async Task TryGetByIdAsyncReturnsNullWhenNoRawFileWithIdExists()
    {
        var rawFile = await _repository.TryGetByIdAsync(1);
        Assert.Null(rawFile);
    }

    [Fact]
    public async Task TryGetByIdAsyncReturnsRawFileWhenRawFileWithIdExists()
    {
        var rawFile = new RawFile
        {
            Id = 1,
            Name = "test",
            Path = "test",
            ConversionStatus = ConversionStatus.NotConverted
        };
        _context.RawFiles.Add(rawFile);
        await _context.SaveChangesAsync();
        var rawFileFromRepository = await _repository.TryGetByIdAsync(1);
        Assert.Equal(rawFile, rawFileFromRepository);
    }

    [Fact]
    public async Task TryGetByPathAsyncReturnsNullWhenNoRawFileWithPathExists()
    {
        var rawFile = await _repository.TryGetByPathAsync("test");
        Assert.Null(rawFile);
    }

    [Fact]
    public async Task TryGetByPathAsyncReturnsRawFileWhenRawFileWithPathExists()
    {
        var rawFile = new RawFile
        {
            Id = 1,
            Name = "test",
            Path = "test",
            ConversionStatus = ConversionStatus.NotConverted
        };
        _context.RawFiles.Add(rawFile);
        await _context.SaveChangesAsync();
        var rawFileFromRepository = await _repository.TryGetByPathAsync("test");
        Assert.Equal(rawFile, rawFileFromRepository);
    }
}
