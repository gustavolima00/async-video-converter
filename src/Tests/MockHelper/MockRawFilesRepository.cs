using Repositories;
using Repositories.Postgres;

namespace Tests.MockHelper;

public class MockRawFilesRepository : RawFilesRepository, IRawFilesRepository
{
  public MockRawFilesRepository() : base(new DatabaseContext(new PostgresConfiguration { UseInMemoryDatabase = true }))
  {
  }
}
