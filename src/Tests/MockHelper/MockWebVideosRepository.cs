using Repositories;
using Repositories.Postgres;

namespace Tests.MockHelper;

public class MockWebVideosRepository : WebVideosRepository, IWebVideosRepository
{
  public MockWebVideosRepository() : base(new DatabaseContext(new PostgresConfiguration { UseInMemoryDatabase = true }))
  {
  }
}
