using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using robot_project_v3.Database.DbContext;
using robot_project_v3.Database.Repositories;
using Serilog;

namespace robot_project_v3.Database;

public static class ServiceDatabaseExtension
{
    public static IServiceCollection AddStrategyDbContext(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<MongoDbContext>();

        services.AddSingleton<IStrategyFileRepository, StrategyFileRepository>();
        
        return services;
    }
}