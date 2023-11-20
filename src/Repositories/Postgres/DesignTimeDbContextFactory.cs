using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Repositories.Postgres;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
        if(string.IsNullOrEmpty(connectionString))
        {
            throw new Exception("POSTGRES_CONNECTION_STRING environment variable is not set");
        }
        optionsBuilder.UseNpgsql(connectionString);

        return new DatabaseContext(new PostgresConfiguration { ConnectionString = connectionString });
    }
}
