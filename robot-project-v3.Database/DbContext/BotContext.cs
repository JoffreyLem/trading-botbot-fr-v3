using Microsoft.EntityFrameworkCore;
using robot_project_v3.Database.Modeles;

namespace robot_project_v3.Database.DbContext;

public class StrategyContext(DbContextOptions<StrategyContext> options)
    : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<StrategyFile> StrategyFiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}