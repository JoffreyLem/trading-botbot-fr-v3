using Microsoft.Extensions.Configuration;
using robot_project_v3.Database.Modeles;

namespace robot_project_v3.Database.DbContext;

using MongoDB.Driver;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var mongoSettings = configuration.GetSection("MongoDb");
        var connectionString = mongoSettings["ConnectionString"];
        var databaseName = mongoSettings["DatabaseName"];

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<StrategyFile> StrategyFiles => _database.GetCollection<StrategyFile>("StrategyFiles");
}
