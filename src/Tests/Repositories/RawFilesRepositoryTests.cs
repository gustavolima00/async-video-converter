using Xunit;
using System.Threading.Tasks;
using Repositories;
using Repositories.Postgres;
using Repositories.Models;
using System;
namespace Tests.Repositories;

public class RawVideosRepositoriesTests
{
    private readonly IRawVideosRepository _repository;
    private readonly DatabaseContext _context;
    public RawVideosRepositoriesTests()
    {
        var postgresConfiguration = new PostgresConfiguration
        {
            UseInMemoryDatabase = true
        };
        _context = new DatabaseContext(postgresConfiguration);
        _repository = new RawVideosRepository(_context);
    }

    [Fact]
    public async Task TryGetByIdAsyncReturnsNullWhenNoRawVideoWithIdExists()
    {
        var rawFile = await _repository.TryGetByIdAsync(1);
        Assert.Null(rawFile);
    }

    [Fact]
    public async Task TryGetByIdAsyncReturnsRawVideoWhenRawVideoWithIdExists()
    {
        var rawFile = new RawVideo
        {
            Id = 1,
            Name = "test",
            Path = "test",
            ConversionStatus = ConversionStatus.NotConverted
        };
        _context.RawVideos.Add(rawFile);
        await _context.SaveChangesAsync();
        var rawFileFromRepository = await _repository.TryGetByIdAsync(1);
        Assert.Equal(rawFile, rawFileFromRepository);
    }

    [Fact]
    public async Task TryGetByPathAsyncReturnsNullWhenNoRawVideoWithPathExists()
    {
        var rawFile = await _repository.TryGetByPathAsync("test");
        Assert.Null(rawFile);
    }

    [Fact]
    public async Task TryGetByPathAsyncReturnsRawVideoWhenRawVideoWithPathExists()
    {
        var rawFile = new RawVideo
        {
            Id = 1,
            Name = "test",
            Path = "test",
            ConversionStatus = ConversionStatus.NotConverted
        };
        _context.RawVideos.Add(rawFile);
        await _context.SaveChangesAsync();
        var rawFileFromRepository = await _repository.TryGetByPathAsync("test");
        Assert.Equal(rawFile, rawFileFromRepository);
    }

    [Fact]
    public async Task CreateOrReplaceByPathAsyncCreatesRawVideoWhenNoRawVideoWithPathExists()
    {
        var rawFile = await _repository.CreateOrReplaceAsync(
            new RawVideo
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
    public async Task CreateOrReplaceByPathAsyncReplacesRawVideoWhenRawVideoWithPathExists()
    {
        var rawFile = new RawVideo
        {
            Id = 1,
            Name = "test",
            Path = "test",
            ConversionStatus = ConversionStatus.NotConverted
        };
        _context.RawVideos.Add(rawFile);
        await _context.SaveChangesAsync();
        var rawFileFromRepository = await _repository.CreateOrReplaceAsync(
            new RawVideo
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
        var rawFile = new RawVideo
        {
            Path = "test",
            ConversionStatus = ConversionStatus.NotConverted,
            UserUuid = Guid.NewGuid()
        };
        _context.RawVideos.Add(rawFile);
        await _context.SaveChangesAsync();
        var rawFileFromRepository = await _repository.CreateOrReplaceAsync(
            new RawVideo
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
        var rawFile = new RawVideo
        {
            Id = 1,
            Name = "test",
            Path = "test",
            ConversionStatus = ConversionStatus.NotConverted
        };
        _context.RawVideos.Add(rawFile);
        await _context.SaveChangesAsync();
        await _repository.UpdateConversionStatusAsync(1, ConversionStatus.Converted);
        var rawFileFromRepository = await _repository.TryGetByIdAsync(1) ?? throw new InvalidOperationException();
        Assert.Equal(ConversionStatus.Converted, rawFileFromRepository.ConversionStatus);
    }

    [Fact]
    public async Task UpdateConversionStatusAsyncThrowsWhenNoRawVideoWithIdExists()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _repository.UpdateConversionStatusAsync(1, ConversionStatus.Converted));
    }
}
