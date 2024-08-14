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
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<StrategyContext>(options =>
            options
                .UseMySql(connectionString, new MariaDbServerVersion(ServerVersion.AutoDetect(connectionString)),
                    builder =>
                        builder.EnableRetryOnFailure(
                            2,
                            TimeSpan.FromSeconds(10),
                            null))
                .LogTo(message =>
                {
                    Log.Logger.Information(message);
                    Console.WriteLine(message);
                }, LogLevel.Information)
                .EnableDetailedErrors());

        services.AddSingleton<IStrategyFileRepository, StrategyFileRepository>();


        return services;
    }
}