using Xunit;
using System.Threading.Tasks;
using Repositories;
using Repositories.Postgres;
using Repositories.Models;
using System;
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

    [Fact]
    public async Task CreateOrReplaceByPathAsyncCreatesRawFileWhenNoRawFileWithPathExists()
    {
        var rawFile = await _repository.CreateOrReplaceAsync(
            new RawFile
            {
                Path = "test",
                UserUuid = Guid.NewGuid(),
            }
        );
        Assert.Equal("test", rawFile.Name);
        Assert.Equal("test", rawFile.Path);
        Assert.Equal(ConversionStatus.NotConverted, rawFile.ConversionStatus);
    }

    [Fact]
    public async Task CreateOrReplaceByPathAsyncReplacesRawFileWhenRawFileWithPathExists()
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
        var rawFileFromRepository = await _repository.CreateOrReplaceAsync(
            new RawFile
            {
                Path = "test",
                UserUuid = Guid.NewGuid(),
            }
        );
        Assert.Equal("test2", rawFileFromRepository.Name);
        Assert.Equal("test", rawFileFromRepository.Path);
        Assert.Equal(ConversionStatus.NotConverted, rawFileFromRepository.ConversionStatus);
    }

    [Fact]
    public async Task CreateOrReplaceByPathWithDifferentUser()
    {
        var rawFile = new RawFile
        {
            Path = "test",
            ConversionStatus = ConversionStatus.NotConverted,
            UserUuid = Guid.NewGuid()
        };
        _context.RawFiles.Add(rawFile);
        await _context.SaveChangesAsync();
        var rawFileFromRepository = await _repository.CreateOrReplaceAsync(
            new RawFile
            {
                Path = "test",
                UserUuid = Guid.NewGuid(),
            }
        );
        Assert.Equal("test", rawFileFromRepository.Name);
        Assert.Equal("test", rawFileFromRepository.Path);
        Assert.Equal(ConversionStatus.NotConverted, rawFileFromRepository.ConversionStatus);
    }

    [Fact]
    public async Task UpdateConversionStatusAsyncUpdatesConversionStatus()
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
        await _repository.UpdateConversionStatusAsync(1, ConversionStatus.Converted);
        var rawFileFromRepository = await _repository.TryGetByIdAsync(1) ?? throw new InvalidOperationException();
        Assert.Equal(ConversionStatus.Converted, rawFileFromRepository.ConversionStatus);
    }

    [Fact]
    public async Task UpdateConversionStatusAsyncThrowsWhenNoRawFileWithIdExists()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _repository.UpdateConversionStatusAsync(1, ConversionStatus.Converted));
    }
}
