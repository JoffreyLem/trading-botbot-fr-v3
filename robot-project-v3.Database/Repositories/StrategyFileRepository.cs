using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using robot_project_v3.Database.DbContext;
using robot_project_v3.Database.Modeles;

namespace robot_project_v3.Database.Repositories;

public interface IStrategyFileRepository
{
    Task<List<StrategyFile>> GetAllAsync();

    Task<StrategyFile> GetByIdAsync(int id);
    Task AddAsync(StrategyFile strategyFile);
    Task UpdateAsync(StrategyFile strategyFile);
    Task DeleteAsync(int id);
}
public class StrategyFileRepository(IServiceScopeFactory scopeFactory) : IStrategyFileRepository
{
    public async Task<List<StrategyFile>> GetAllAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StrategyContext>();
        return await dbContext.StrategyFiles.ToListAsync();
    }


    public async Task<StrategyFile> GetByIdAsync(int id)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StrategyContext>();
        return await dbContext.StrategyFiles.FindAsync(id);
    }

    public async Task AddAsync(StrategyFile strategyFile)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StrategyContext>();
        dbContext.StrategyFiles.Add(strategyFile);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(StrategyFile strategyFile)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StrategyContext>();
        dbContext.StrategyFiles.Update(strategyFile);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StrategyContext>();
        var strategyFile = await dbContext.StrategyFiles.FindAsync(id);
        if (strategyFile != null)
        {
            dbContext.StrategyFiles.Remove(strategyFile);
            await dbContext.SaveChangesAsync();
        }
    }
}