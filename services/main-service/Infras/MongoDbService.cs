using MongoDB.Driver;
using MongoDB.Bson;

namespace MainService.Infras;
public class MongoDbService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MongoDbService> _logger;
    private IMongoDatabase? _database;
    private IMongoClient? _mongoClient;
    public IMongoClient MongoClient => _mongoClient ?? throw new InvalidOperationException("MongoClient not initialized. Call Initiate() first.");
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
            _mongoClient = new MongoClient(mongoUrl);

            var dbName = mongoUrl.DatabaseName ?? "taskflow";
            _database = _mongoClient.GetDatabase(dbName);

            _logger.LogInformation("MongoDB connection successful!");
        }
        catch (Exception ex)
        {
            _logger.LogError($"MongoDB connection failed: {ex.Message}");
        }
    }

}
