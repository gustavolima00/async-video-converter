using Xunit;
using System.Threading.Tasks;
using Repositories;
using Repositories.Postgres;
using Repositories.Models;
using System;
namespace Tests.Repositories;

public class ConvertedVideosRepositoriesTests
{
    private readonly IConvertedVideosRepository _repository;
    private readonly DatabaseContext _context;
    public ConvertedVideosRepositoriesTests()
    {
        var postgresConfiguration = new PostgresConfiguration
        {
            UseInMemoryDatabase = true
        };
        _context = new DatabaseContext(postgresConfiguration);
        _repository = new ConvertedVideosRepository(_context);
    }


    [Fact]
    public async Task TryGetByIdAsyncReturnsNullWhenNoConvertedVideoWithIdExists()
    {
        var webVideo = await _repository.TryGetByIdAsync(1);
        Assert.Null(webVideo);
    }

    [Fact]
    public async Task TryGetByIdAsyncReturnsConvertedVideoWhenConvertedVideoWithIdExists()
    {
        var webVideo = new ConvertedVideo
        {
            Id = 1,
            Name = "test",
            Path = "test",
            Link = "test",
        };
        _context.ConvertedVideos.Add(webVideo);
        await _context.SaveChangesAsync();
        var webVideoFromRepository = await _repository.TryGetByIdAsync(1);
        Assert.Equal(webVideo, webVideoFromRepository);
    }

    [Fact]
    public async Task GetAllAsyncReturnsEmptyListWhenNoConvertedVideosExist()
    {
        var webVideos = await _repository.GetAllAsync();
        Assert.Empty(webVideos);
    }

    [Fact]
    public async Task GetAllAsyncReturnsAllConvertedVideos()
    {
        var webVideo1 = new ConvertedVideo
        {
            Id = 1,
            Name = "test",
            Path = "test",
            Link = "test",
        };
        var webVideo2 = new ConvertedVideo
        {
            Id = 2,
            Name = "test",
            Path = "test",
            Link = "test",
        };
        _context.ConvertedVideos.Add(webVideo1);
        _context.ConvertedVideos.Add(webVideo2);
        await _context.SaveChangesAsync();
        var webVideosFromRepository = await _repository.GetAllAsync();
        Assert.Equal(new[] { webVideo1, webVideo2 }, webVideosFromRepository);
    }

    [Fact]
    public async Task CreateOrReplaceAsyncCreatesConvertedVideoWhenNoConvertedVideoWithPathExists()
    {
        var webVideo = new ConvertedVideo
        {
            Id = 1,
            Name = "test",
            Path = "test",
            Link = "test",
        };
        var webVideoFromRepository = await _repository.CreateOrReplaceAsync(webVideo);
        Assert.Equal(webVideo, webVideoFromRepository);
    }

    [Fact]
    public async Task CreateOrReplaceAsyncReplacesConvertedVideoWhenConvertedVideoWithPathExists()
    {
        var webVideo = new ConvertedVideo
        {
            Name = "test",
            Path = "test",
            Link = "test",
        };
        _context.ConvertedVideos.Add(webVideo);
        await _context.SaveChangesAsync();

        var newConvertedVideo = new ConvertedVideo
        {
            Name = "test_updated",
            Path = "test",
            Link = "test_updated",
        };

        var webVideoFromRepository = await _repository.CreateOrReplaceAsync(newConvertedVideo);
        Assert.NotEqual(webVideo.Id, webVideoFromRepository.Id); // Os Ids devem ser diferentes
        Assert.Equal("test_updated", webVideoFromRepository.Name);
    }


    [Fact]
    public async Task GetConvertedVideoReturnsSubtitles()
    {
        var webVideo = new ConvertedVideo
        {
            Id = 1,
            Name = "test",
            Path = "test",
            Link = "test",
        };
        var subtitle = new ConvertedSubtitle
        {
            Id = 1,
            ConvertedVideoId = 1,
            Path = "test",
            Language = "test",
            Link = "test",
        };
        _context.ConvertedVideos.Add(webVideo);
        _context.ConvertedSubtitles.Add(subtitle);
        await _context.SaveChangesAsync();
        var webVideoFromRepository = await _repository.TryGetByIdAsync(1);
        Assert.Equal(webVideo, webVideoFromRepository);
        if (webVideoFromRepository is null)
        {
            throw new Exception("webVideoFromRepository is null");
        }
        Assert.Equal(new[] { subtitle }, webVideoFromRepository.Subtitles);
    }

}
