using Repositories;
using Repositories.Postgres;

namespace Tests.MockHelper;

public class MockConvertedVideosRepository : ConvertedVideosRepository, IConvertedVideosRepository
{
  public MockConvertedVideosRepository() : base(new DatabaseContext(new PostgresConfiguration { UseInMemoryDatabase = true }))
  {
  }
}
