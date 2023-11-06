using Xunit;
using System.Threading.Tasks;
using Repositories;
using Repositories.Postgres;
using Repositories.Models;
using System;
using System.Linq;
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
    public async Task CreateOrReplaceByPathAsyncCreatesRawVideoWhenNoRawVideoWithPathExists()
    {
        var rawFile = await _repository.CreateOrReplaceAsync(
            new RawVideo
            {
                Path = "test",
                Name = "test",
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
        var userUuid = Guid.NewGuid();
        await _repository.CreateOrReplaceAsync(new()
        {
            Id = 1,
            Name = "test",
            Path = "test",
            UserUuid = userUuid,
            ConversionStatus = ConversionStatus.NotConverted
        });
        Assert.Equal(1, _context.RawVideos.Count());


        var rawFileFromRepository = await _repository.CreateOrReplaceAsync(
            new RawVideo
            {
                Path = "test",
                Name = "test2",
                UserUuid = userUuid,
            }
        );
        Assert.Equal("test2", rawFileFromRepository.Name);
        Assert.Equal("test", rawFileFromRepository.Path);
        Assert.Equal(ConversionStatus.NotConverted, rawFileFromRepository.ConversionStatus);
        Assert.Equal(1, _context.RawVideos.Count());
    }

    [Fact]
    public async Task CreateOrReplaceByPathWithDifferentUser()
    {
        await _repository.CreateOrReplaceAsync(new()
        {
            Path = "test",
            ConversionStatus = ConversionStatus.NotConverted,
            UserUuid = Guid.NewGuid()
        });
        Assert.Equal(1, _context.RawVideos.Count());

        var rawFileFromRepository = await _repository.CreateOrReplaceAsync(
            new RawVideo
            {
                Path = "test",
                Name = "test",
                UserUuid = Guid.NewGuid(),
            }
        );
        Assert.Equal("test", rawFileFromRepository.Name);
        Assert.Equal("test", rawFileFromRepository.Path);
        Assert.Equal(ConversionStatus.NotConverted, rawFileFromRepository.ConversionStatus);
        Assert.Equal(2, _context.RawVideos.Count());
    }


    [Fact]
    public async Task TryGetAsyncReturnsNullWhenNoRawVideoWithIdExists()
    {
        var rawFileById = await _repository.TryGetByIdAsync(1);
        Assert.Null(rawFileById);

        var rawFileByPath = await _repository.TryGetByPathAsync("test");
        Assert.Null(rawFileByPath);
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
        await _repository.CreateOrReplaceAsync(rawFile);
        var rawFileFromRepository = await _repository.TryGetByIdAsync(1);
        Assert.Equal(rawFile, rawFileFromRepository);
    }

    [Fact]
    public async Task UpdateConversionStatusAsyncUpdatesConversionStatus()
    {
        await _repository.CreateOrReplaceAsync(new()
        {
            Id = 1,
            Name = "test",
            Path = "test",
            ConversionStatus = ConversionStatus.NotConverted
        });
        Assert.Equal(1, _context.RawVideos.Count());
        Assert.Equal(ConversionStatus.NotConverted, _context.RawVideos.First().ConversionStatus);


        await _repository.UpdateConversionStatusAsync(1, ConversionStatus.Converted);
        Assert.Equal(1, _context.RawVideos.Count());
        Assert.Equal(ConversionStatus.Converted, _context.RawVideos.First().ConversionStatus);
    }

    [Fact]
    public async Task UpdateConversionStatusAsyncThrowsWhenNoRawVideoWithIdExists()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _repository.UpdateConversionStatusAsync(1, ConversionStatus.Converted));
    }

    // Subtitles
    [Fact]
    public async Task CreateOrReplaceRawSubtitleAsyncCreatesRawSubtitleWhenNoRawSubtitleWithPathExists()
    {
        var userUuid = Guid.NewGuid();
        var rawSubtitle = await _repository.CreateOrReplaceRawSubtitleAsync(
            new RawSubtitle
            {
                Path = "test",
                Name = "test",
                UserUuid = userUuid,
            }
        );
        var rawSubtitleFromContext = _context.RawSubtitles.First();
        Assert.Equal(rawSubtitleFromContext, rawSubtitle);
        Assert.Equal(1, _context.RawSubtitles.Count());
    }

    [Fact]
    public async Task CreateOrReplaceRawSubtitleAsyncReplacesRawSubtitleWhenRawSubtitleWithPathExists()
    {
        var userUuid = Guid.NewGuid();
        await _repository.CreateOrReplaceRawSubtitleAsync(new()
        {
            Id = 1,
            Name = "test",
            Path = "test",
            UserUuid = userUuid,
            ConversionStatus = ConversionStatus.NotConverted
        });
        Assert.Equal(1, _context.RawSubtitles.Count());

        var rawSubtitle = await _repository.CreateOrReplaceRawSubtitleAsync(
            new RawSubtitle
            {
                Path = "test",
                Name = "test2",
                UserUuid = userUuid,
            }
        );

        var rawSubtitleFromContext = _context.RawSubtitles.First();

        Assert.Equal(1, _context.RawSubtitles.Count());
        Assert.Equal(rawSubtitleFromContext, rawSubtitle);
    }

    [Fact]
    public async Task CreateOrReplaceRawSubtitleAsyncWithDifferentUser()
    {
        await _repository.CreateOrReplaceRawSubtitleAsync(new()
        {
            Path = "test",
            ConversionStatus = ConversionStatus.NotConverted,
            UserUuid = Guid.NewGuid()
        });
        Assert.Equal(1, _context.RawSubtitles.Count());

        await _repository.CreateOrReplaceRawSubtitleAsync(
            new RawSubtitle
            {
                Path = "test",
                Name = "test",
                UserUuid = Guid.NewGuid(),
            }
        );
        Assert.Equal(2, _context.RawSubtitles.Count());
    }

    [Fact]
    public async Task TryGetRawSubtitleAsyncReturnsNullWhenNoRawSubtitleWithIdExists()
    {
        var rawSubtitleById = await _repository.TryGetSubtitleByIdAsync(1);
        Assert.Null(rawSubtitleById);

        var rawSubtitleByPath = await _repository.TryGetSubtitleByPathAsync("test");
        Assert.Null(rawSubtitleByPath);
    }

    [Fact]
    public async Task TryGetRawSubtitleAsyncReturnsRawSubtitleWhenRawSubtitleWithIdExists()
    {
        var rawSubtitle = new RawSubtitle
        {
            Id = 1,
            Name = "test",
            Path = "test",
            ConversionStatus = ConversionStatus.NotConverted
        };
        await _repository.CreateOrReplaceRawSubtitleAsync(rawSubtitle);
        var rawSubtitleFromRepository = await _repository.TryGetSubtitleByIdAsync(1);
        Assert.Equal(rawSubtitle, rawSubtitleFromRepository);
    }

    [Fact]
    public async Task UpdateRawSubtitleConversionStatusAsyncUpdatesConversionStatus()
    {
        await _repository.CreateOrReplaceRawSubtitleAsync(new()
        {
            Id = 1,
            Name = "test",
            Path = "test",
            ConversionStatus = ConversionStatus.NotConverted
        });
        Assert.Equal(1, _context.RawSubtitles.Count());
        Assert.Equal(ConversionStatus.NotConverted, _context.RawSubtitles.First().ConversionStatus);
    }

    [Fact]
    public async Task UpdateRawSubtitleConversionStatusAsyncThrowsWhenNoRawSubtitleWithIdExists()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _repository.UpdateSubtitleConversionStatusAsync(1, ConversionStatus.Converted));
    }
}
