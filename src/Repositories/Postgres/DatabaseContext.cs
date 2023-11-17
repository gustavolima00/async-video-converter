using Microsoft.EntityFrameworkCore;
using Repositories.Models;

namespace Repositories.Postgres;

public class DatabaseContext : DbContext
{
  public DbSet<WebhookUser> WebhookUsers { get; set; }
  public DbSet<RawVideo> RawVideos { get; set; }
  public DbSet<RawSubtitle> RawSubtitles { get; set; }
  public DbSet<ConvertedVideo> ConvertedVideos { get; set; }
  public DbSet<ConvertedSubtitle> ConvertedSubtitles { get; set; }
  public DbSet<ConvertedVideoTrack> ConvertedVideoTracks { get; set; }

  private readonly PostgresConfiguration _configuration;

  public bool SupportTransaction => !_configuration.UseInMemoryDatabase;

  public DatabaseContext(PostgresConfiguration postgresConfiguration)
  {
    _configuration = postgresConfiguration;
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<WebhookUser>();
    modelBuilder.Entity<RawVideo>();
    modelBuilder.Entity<RawSubtitle>();
    modelBuilder.Entity<ConvertedVideo>();
    modelBuilder.Entity<ConvertedSubtitle>();
    modelBuilder.Entity<ConvertedVideoTrack>();
  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    if (_configuration.UseInMemoryDatabase)
      optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
    else
      optionsBuilder.UseNpgsql(_configuration.ConnectionString);
  }
}
