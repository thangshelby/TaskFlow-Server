using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MainService.Infras;
public class MongoDbService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MongoDbService> _logger;
    private IMongoDatabase? _database;
    public MongoDbService(IConfiguration configuration, ILogger<MongoDbService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        Initiate();
    }

    public IMongoDatabase Database => _database ?? throw new InvalidOperationException("Database not initialized. Call Initiate() first.");

    public void Initiate()
    {
        try
        {
            var connectionStr = _configuration.GetConnectionString("MongoDb");
            var mongoUrl = MongoUrl.Create(connectionStr);
            var mongoClient = new MongoClient(mongoUrl);
            _database = mongoClient.GetDatabase(mongoUrl.DatabaseName);

            _logger.LogInformation("MongoDB connection successful!");
        }
        catch (Exception ex)
        {
            _logger.LogError($"MongoDB connection failed: {ex.Message}");
        }
    }
}
