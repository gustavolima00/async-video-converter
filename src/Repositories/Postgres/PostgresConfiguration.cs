namespace Repositories.Postgres;

public class PostgresConfiguration
{
    public string ConnectionString { get; set; } = "";
    public bool UseInMemoryDatabase { get; set; } = false;
}
