namespace Repositories;

public interface IRawFilesRepository
{

}

class RawFilesRepository : IRawFilesRepository
{
    private readonly IDatabaseService _databaseService;
    public RawFilesRepository(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }
}