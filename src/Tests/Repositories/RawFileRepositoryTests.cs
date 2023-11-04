using Xunit;
using System.Threading.Tasks;
using Repositories;
using Repositories.Postgres;
namespace Tests.Repositories;

public class RawFilesRepositoriesTests
{
    private readonly IRawFilesRepository _repository;
    private readonly DatabaseContext _context;
    public RawFilesRepositoriesTests()
    {
        var postgresConfiguration = new PostgresConfiguration
        {
            UseInMemoryDatabase = true
        };
        var databaseConnection = new DatabaseConnection(postgresConfiguration);
        _context = new DatabaseContext(postgresConfiguration);
        _repository = new RawFilesRepository(databaseConnection, _context);
    }

    [Fact]
    public async Task TryGetByIdAsyncReturnsNullWhenNoRawFileWithIdExists()
    {
        var rawFile = await _repository.TryGetByIdAsync(1);
        Assert.Null(rawFile);
    }
}
