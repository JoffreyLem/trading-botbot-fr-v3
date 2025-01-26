using MongoDB.Driver;
using robot_project_v3.Database.DbContext;
using robot_project_v3.Database.Modeles;

namespace robot_project_v3.Database.Repositories;

public interface IStrategyFileRepository
{
    Task<List<StrategyFile>> GetAllAsync();
    Task<StrategyFile?> GetByIdAsync(string id);
    Task AddAsync(StrategyFile strategyFile);
    Task UpdateAsync(StrategyFile strategyFile);
    Task DeleteAsync(string id);
}

public class StrategyFileRepository : IStrategyFileRepository
{
    private readonly IMongoCollection<StrategyFile> _strategyFiles;

    public StrategyFileRepository(MongoDbContext context)
    {
        _strategyFiles = context.StrategyFiles;
    }

    public async Task<List<StrategyFile>> GetAllAsync()
    {
        return await _strategyFiles.Find(_ => true).ToListAsync();
    }

    public async Task<StrategyFile?> GetByIdAsync(string id)
    {
        return await _strategyFiles.Find(s => s.Id == id).FirstOrDefaultAsync();
    }

    public async Task AddAsync(StrategyFile strategyFile)
    {
        await _strategyFiles.InsertOneAsync(strategyFile);
    }

    public async Task UpdateAsync(StrategyFile strategyFile)
    {
        await _strategyFiles.ReplaceOneAsync(s => s.Id == strategyFile.Id, strategyFile);
    }

    public async Task DeleteAsync(string id)
    {
        await _strategyFiles.DeleteOneAsync(s => s.Id == id);
    }
}