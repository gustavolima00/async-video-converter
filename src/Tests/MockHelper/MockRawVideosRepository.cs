using Repositories;
using Repositories.Postgres;

namespace Tests.MockHelper;

public class MockRawVideosRepository : RawVideosRepository, IRawVideosRepository
{
  public MockRawVideosRepository() : base(new DatabaseContext(new PostgresConfiguration { UseInMemoryDatabase = true }))
  {
  }
}
