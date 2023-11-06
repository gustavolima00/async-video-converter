using Microsoft.EntityFrameworkCore;
using Repositories.Models;

namespace Repositories.Postgres;

public class DatabaseContext : DbContext
{
  public DbSet<RawVideo> RawVideos { get; set; }
  public DbSet<WebVideo> WebVideos { get; set; }
  public DbSet<WebVideoSubtitle> WebVideoSubtitles { get; set; }
  public DbSet<WebhookUser> WebhookUsers { get; set; }

  private readonly PostgresConfiguration _configuration;

  public bool SupportTransaction => !_configuration.UseInMemoryDatabase;

  public DatabaseContext(PostgresConfiguration postgresConfiguration)
  {
    _configuration = postgresConfiguration;
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<RawVideo>(entity =>
    {
      if (_configuration.UseInMemoryDatabase)
      {
        modelBuilder.Entity<RawVideo>().Ignore(rf => rf.Metadata);
      }
      else
      {
        entity.Property(e => e.Metadata)
                  .HasColumnType("jsonb");
      }
    });

    modelBuilder.Entity<WebVideo>(entity =>
    {
      if (_configuration.UseInMemoryDatabase)
      {
        modelBuilder.Entity<WebVideo>().Ignore(wv => wv.Metadata);
      }
      else
      {
        entity.Property(e => e.Metadata)
                  .HasColumnType("jsonb");
      }

    });

    modelBuilder.Entity<WebVideoSubtitle>(entity =>
    {
      if (_configuration.UseInMemoryDatabase)
      {
        modelBuilder.Entity<WebVideoSubtitle>().Ignore(wvs => wvs.Metadata);
      }
      else
      {
        entity.Property(e => e.Metadata)
                  .HasColumnType("jsonb");
      }
    });

    modelBuilder.Entity<WebhookUser>();
  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    if (_configuration.UseInMemoryDatabase)
      optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
    else
      optionsBuilder.UseNpgsql(_configuration.ConnectionString);
  }
}
