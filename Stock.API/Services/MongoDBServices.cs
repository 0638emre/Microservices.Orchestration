using MongoDB.Driver;

namespace Stock.API.Services;

public class MongoDBServices
{
    readonly IMongoDatabase _database;

    public MongoDBServices(IConfiguration configuration)
    {
        MongoClient client = new(configuration.GetConnectionString("OrchestrationStockDB"));
        _database = client.GetDatabase("OrchestrationStockDB");
    }

    public IMongoCollection<T> GetCollection<T>() => _database.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
}