using Microsoft.EntityFrameworkCore;
using Repositories.Models;

namespace Repositories.Postgres;

public class DatabaseContext : DbContext
{
  public DbSet<RawFile> RawFiles { get; set; }
  private readonly PostgresConfiguration _configuration;

  public bool SupportTransaction => !_configuration.UseInMemoryDatabase;

  public DatabaseContext(PostgresConfiguration postgresConfiguration)
  {
    _configuration = postgresConfiguration;
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<RawFile>(entity =>
    {
      if (_configuration.UseInMemoryDatabase)
      {
        modelBuilder.Entity<RawFile>().Ignore(rf => rf.Metadata);
      }
      else
      {
        entity.Property(e => e.Metadata)
                  .HasColumnType("jsonb");
      }
    });
  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    if (_configuration.UseInMemoryDatabase)
      optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
    else
      optionsBuilder.UseNpgsql(_configuration.ConnectionString);
  }
}
