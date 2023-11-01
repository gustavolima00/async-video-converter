using Microsoft.EntityFrameworkCore;
namespace Repositories.Postgres;

class BaseDbContext : DbContext
{
    private readonly PostgresConfiguration _configuration;

    public BaseDbContext(PostgresConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.ConnectionString);
    }
}