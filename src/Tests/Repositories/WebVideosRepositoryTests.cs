using Xunit;
using System.Threading.Tasks;
using Repositories;
using Repositories.Postgres;
using Repositories.Models;
using System;
namespace Tests.Repositories;

public class WebVideosRepositoriesTests
{
    private readonly IWebVideosRepository _repository;
    private readonly DatabaseContext _context;
    public WebVideosRepositoriesTests()
    {
        var postgresConfiguration = new PostgresConfiguration
        {
            UseInMemoryDatabase = true
        };
        _context = new DatabaseContext(postgresConfiguration);
        _repository = new WebVideosRepository(_context);
    }


    [Fact]
    public async Task TryGetByIdAsyncReturnsNullWhenNoWebVideoWithIdExists()
    {
        var webVideo = await _repository.TryGetByIdAsync(1);
        Assert.Null(webVideo);
    }

    [Fact]
    public async Task TryGetByIdAsyncReturnsWebVideoWhenWebVideoWithIdExists()
    {
        var webVideo = new WebVideo
        {
            Id = 1,
            Name = "test",
            Path = "test",
            Link = "test",
        };
        _context.WebVideos.Add(webVideo);
        await _context.SaveChangesAsync();
        var webVideoFromRepository = await _repository.TryGetByIdAsync(1);
        Assert.Equal(webVideo, webVideoFromRepository);
    }

    [Fact]
    public async Task GetAllAsyncReturnsEmptyListWhenNoWebVideosExist()
    {
        var webVideos = await _repository.GetAllAsync();
        Assert.Empty(webVideos);
    }

    [Fact]
    public async Task GetAllAsyncReturnsAllWebVideos()
    {
        var webVideo1 = new WebVideo
        {
            Id = 1,
            Name = "test",
            Path = "test",
            Link = "test",
        };
        var webVideo2 = new WebVideo
        {
            Id = 2,
            Name = "test",
            Path = "test",
            Link = "test",
        };
        _context.WebVideos.Add(webVideo1);
        _context.WebVideos.Add(webVideo2);
        await _context.SaveChangesAsync();
        var webVideosFromRepository = await _repository.GetAllAsync();
        Assert.Equal(new[] { webVideo1, webVideo2 }, webVideosFromRepository);
    }

    [Fact]
    public async Task CreateOrReplaceAsyncCreatesWebVideoWhenNoWebVideoWithPathExists()
    {
        var webVideo = new WebVideo
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
    public async Task CreateOrReplaceAsyncReplacesWebVideoWhenWebVideoWithPathExists()
    {
        var webVideo = new WebVideo
        {
            Name = "test",
            Path = "test",
            Link = "test",
        };
        _context.WebVideos.Add(webVideo);
        await _context.SaveChangesAsync();

        var newWebVideo = new WebVideo
        {
            Name = "test_updated",
            Path = "test",
            Link = "test_updated",
        };

        var webVideoFromRepository = await _repository.CreateOrReplaceAsync(newWebVideo);
        Assert.NotEqual(webVideo.Id, webVideoFromRepository.Id); // Os Ids devem ser diferentes
        Assert.Equal("test_updated", webVideoFromRepository.Name);
    }


    [Fact]
    public async Task GetWebVideoReturnsSubtitles()
    {
        var webVideo = new WebVideo
        {
            Id = 1,
            Name = "test",
            Path = "test",
            Link = "test",
        };
        var subtitle = new WebVideoSubtitle
        {
            Id = 1,
            WebVideoId = 1,
            Path = "test",
            Language = "test",
            Link = "test",
        };
        _context.WebVideos.Add(webVideo);
        _context.WebVideoSubtitles.Add(subtitle);
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
